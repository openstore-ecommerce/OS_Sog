using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using OS_Sog.Models;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using DotNetNuke.Entities.Portals;

namespace OS_Sog
{
    public class OS_SogPaymentProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.PaymentsInterface
    {
        public override string Paymentskey { get; set; }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_Sogpayment", "OS_SogPAYMENT", Utils.GetCurrentCulture());
            var templateName = info.GetXmlProperty("genxml/textbox/checkouttemplate");
            var passSettings = info.ToDictionary();
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }
            var templ = NBrightBuyUtils.RazorTemplRender(templateName, 0, "", info, "/DesktopModules/NBright/OS_Sog", "config", Utils.GetCurrentCulture(), passSettings);

            return templ;
        }

        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        public override string RedirectForPayment(OrderData orderData)
        {
            orderData.OrderStatus = "020";
            orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "");
            orderData.PurchaseInfo.Lang = Utils.GetCurrentCulture();
            orderData.SavePurchaseData();
            try
            {
                var req = HttpContext.Current.Request;

                //get response from post body
                var paymentResponse = JsonConvert.DeserializeObject<PaymentResponse>(req.Form.Get("kr-answer").ToString());
                
                //check kr-answer object signature
                var hash = req.Form.Get("kr-hash");
                var hashKey = req.Form.Get("kr-hash-key"); //sha256_hmac (browser return) or password (IPN)
                var hashAlgorythm = req.Form.Get("kr-hash-algorithm"); //always sha256_hmac

                //get plugin settings
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetPluginSinglePageData("OS_Sogpayment", "OS_SogPAYMENT", Utils.GetCurrentCulture());
                var hashkey = NBrightCore.common.Security.Decrypt(PortalController.Instance.GetCurrentSettings().GUID.ToString(), info.GetXmlProperty("genxml/textbox/hashkey"));

                var shaKeyBytes = System.Text.Encoding.UTF8.GetBytes(hashkey);

                // see: https://sogecommerce.societegenerale.eu/doc/en-EN/rest/V4.0/kb/payment_done.html

                //validate the answer hash
                using (var shaAlgorithm = new System.Security.Cryptography.HMACSHA256(shaKeyBytes))
                {
                    var answerBytes = System.Text.Encoding.UTF8.GetBytes(req.Form.Get("kr-answer").ToString());
                    var signatureHashBytes = shaAlgorithm.ComputeHash(answerBytes);
                    var signatureHashHex = string.Concat(Array.ConvertAll(signatureHashBytes, b => b.ToString("X2"))).ToLower();

                    if (signatureHashHex != hash) {
                        //Invalid hash key, Return to Payment Tab with Failure message;
                        //TODO: consider logging all requests for this path
                        var param = new string[2];
                        param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                        param[1] = "status=0";
                        return Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
                    }
                }

                if (paymentResponse == null)
                {
                    //No payment response, Return to Payment Tab with Failure message;
                    var param = new string[2];
                    param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                    param[1] = "status=0";
                    return Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
                }
                else
                {
                    // 010 = Incomplete, 020 = Waiting for Bank,030 = Cancelled,040 = Payment OK,050 = Payment Not Verified,060 = Waiting for Payment,070 = Waiting for Stock,080 = Waiting,090 = Shipped,010 = Closed,011 = Archived

                    HttpContext.Current.Response.Clear();

                    //TODO: Test IPN payments to see if you need a wrapper here..??
                    //      We may need to have a "IPN enabled" setting

                    // response will come from the embedded form
                    //var response = ProviderUtils.GetChargeResponse(orderData, null);

                    var param = new string[2];
                    param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                    
                    if (paymentResponse.orderStatus == "PAID")
                    {
                        //add external order id, payment id & status to PurchaseInfo for dev reference
                        //orderData.PurchaseInfo.SetXmlProperty("genxml/externalorderid", paymentResponse.answer.orderId);
                        //orderData.PurchaseInfo.SetXmlProperty("genxml/externalpaymentid", paymentResponse.answer.paymentOrderId);
                        //orderData.PurchaseInfo.SetXmlProperty("genxml/externalstatus", paymentResponse.orderStatus);
                        
                        //add the Sog payment uuid to the audit log for admins/managers to reference
                        orderData.AddAuditMessage("Sog Payment ID " + paymentResponse.transactions[0].Uuid, "notes", UserController.Instance.GetCurrentUserInfo().Username, "False");

                        // successful transaction
                        orderData.PaymentOk("040");
                        param[1] = "status=1";

                        NBrightBuyUtils.SendOrderEmail("OrderCreatedClient", orderData.PurchaseInfo.ItemID, "ordercreatedemailsubject");

                    }
                    else {
                        // failed transaction
                        orderData.OrderStatus = "030";
                        param[1] = "status=0";

                        // create error string for output to the order audit log
                        var errorString = "";
                        //if (response.Errors.Count > 0)
                        //{
                        //    foreach (var e in response.Errors)
                        //    {
                        //        errorString += e.Detail;
                        //        errorString += " ";
                        //    };
                        //}
                        
                        //add message for admins to view in the order audit log
                        orderData.AddAuditMessage(errorString, "notes", UserController.Instance.GetCurrentUserInfo().Username, "False");
                    }

                    orderData.SavePurchaseData();
                    HttpContext.Current.Response.Redirect(Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param),false);
                }
            }
            catch (Exception ex)
            {
                // rollback transaction
                // NOTE: The errors returned by the gateway are not shown to the user
                //      DNN admin must be able to review the cart data for a user.
                orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "<div>ERROR: Invalid payment data </div><div>" + ex + "</div>");
                orderData.PaymentFail();
                var param = new string[2];
                param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                param[1] = "status=0";
                HttpContext.Current.Response.Redirect(Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param));
            }

            try
            {
                HttpContext.Current.Response.End();
            }
            catch (Exception)
            {
                // this try/catch to avoid sending error 'ThreadAbortException'  
            }

            return "";
        }

        public override string ProcessPaymentReturn(HttpContext context)
        {
            var orderid = Utils.RequestQueryStringParam(context, "orderid");
            if (Utils.IsNumeric(orderid))
            {
                var orderData = new OrderData(Convert.ToInt32(orderid));
                var status = Utils.RequestQueryStringParam(context, "status");
                if (status == "0")
                {
                    var rtnerr = "";
                    if (orderData.OrderStatus == "020") // check we have a waiting for bank status, IPN may have already altered this. 
                    {
                        rtnerr = orderData.PurchaseInfo.GetXmlProperty("genxml/paymenterror");
                        orderData.PaymentFail();
                    }
                    return GetReturnTemplate(orderData, false, rtnerr);
                }
                // check we have a waiting for bank status (IPN may have altered status already + help stop hack)
                if (orderData.OrderStatus == "020")
                {
                    orderData.PaymentOk("050"); // order paid, but NOT verified
                }
                return GetReturnTemplate(orderData, true, "");
            }
            return "";
        }

        private string GetReturnTemplate(OrderData orderData, bool paymentok, string paymenterror)
        {
            var info = ProviderUtils.GetProviderSettings();
            info.UserId = UserController.Instance.GetCurrentUserInfo().UserID;
            var passSettings = NBrightBuyUtils.GetPassSettings(info);
            if (passSettings.ContainsKey("paymenterror"))
            {
                passSettings.Add("paymenterror", paymenterror);
            }
            var displaytemplate = "payment_ok.cshtml";
            string templ;
            if (paymentok)
            {
                info.SetXmlProperty("genxml/ordernumber", orderData.OrderNumber);
                templ = NBrightBuyUtils.RazorTemplRender(displaytemplate, 0, "", info, "/DesktopModules/NBright/OS_Sog", "config", Utils.GetCurrentCulture(), passSettings);
            }
            else
            {
                displaytemplate = "payment_fail.cshtml";
                templ = NBrightBuyUtils.RazorTemplRender(displaytemplate, 0, "", info, "/DesktopModules/NBright/OS_Sog", "config", Utils.GetCurrentCulture(), passSettings);
            }

            return templ;
        }

    }
}
