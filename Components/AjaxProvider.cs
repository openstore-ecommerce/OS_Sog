using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using DotNetNuke.Entities.Portals;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using OS_Sog.Models;

namespace OS_Sog
{
    public class AjaxProvider : AjaxInterface
    {
        private readonly string _serviceUrl = "https://api-sogecommerce.societegenerale.eu/api-payment/";
        public override string Ajaxkey { get; set; }

        public override string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var lang = NBrightBuyUtils.SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.
            var objCtrl = new NBrightBuyController();

            var strOut = "OS_Sog Ajax Error";

            // NOTE: The paramCmd MUST start with the plugin ref. in lowercase. (links ajax provider to cmd)
            switch (paramCmd)
            {
                case "OS_Sog_savesettings":
                    strOut = objCtrl.SavePluginSinglePageData(context);
                    break;
                case "OS_Sog_selectlang":
                    objCtrl.SavePluginSinglePageData(context);
                    var nextlang = ajaxInfo.GetXmlProperty("genxml/hidden/nextlang");
                    var info = objCtrl.GetPluginSinglePageData("OS_Sogpayment", "OS_SogPAYMENT", nextlang);
                    strOut = NBrightBuyUtils.RazorTemplRender("settingsfields.cshtml", 0, "", info, "/DesktopModules/NBright/OS_Sog", "config", nextlang, StoreSettings.Current.Settings());
                    break;
                case "OS_Sog_tokenize":
                    var settings = ProviderUtils.GetProviderSettings();
                    var userName = NBrightCore.common.Security.Decrypt(PortalSettings.Current.GUID.ToString(), settings.GetXmlProperty("genxml/textbox/username"));
                    var password = NBrightCore.common.Security.Decrypt(PortalSettings.Current.GUID.ToString(), settings.GetXmlProperty("genxml/textbox/productionpassword"));
                    var sandboxMode = settings.GetXmlPropertyBool("genxml/checkbox/sandboxmode");
                    if (sandboxMode)
                    {
                        password = NBrightCore.common.Security.Decrypt(PortalSettings.Current.GUID.ToString(), settings.GetXmlProperty("genxml/textbox/testpassword"));
                    }

                    var stringToEncode = userName + ":" + password;
                    var encodedString = Base64Encode(stringToEncode);

                    //get cart total
                    var cartData = new CartData(PortalSettings.Current.PortalId);
                    if (cartData != null) {
                        double amount;
                        if (double.TryParse(cartData.GetInfo().GetXmlProperty("genxml/total"), out amount)){

                            var currencyCode = settings.GetXmlProperty("genxml/dropdownlist/currencycode");
                            var appliedtotal = cartData.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal");
                            var alreadypaid = cartData.PurchaseInfo.GetXmlPropertyDouble("genxml/alreadypaid");

                            // use the smallest denomination of the Currency being used
                            // ie. when we take 1.00 EUR then we will need to convert to 100
                            var currencyFactor = 100;
                            
                            //TODO: identify currencies that require adjustments to the currency factor
                            if (currencyCode == "JPY")
                            {
                                currencyFactor = 1;
                            }

                            var orderTotal = (long)((appliedtotal - alreadypaid) * currencyFactor);

                            var storeName = StoreSettings.Current.SettingsInfo.GetXmlProperty("genxml/textbox/storename");

                            //build Sog payment request body
                            var body = new TokenRequest();
                            body.amount = orderTotal;
                            //body.customer = new Customer { email = "test@test.com" };
                            body.currency = settings.GetXmlProperty("genxml/dropdownlist/currencycode");
                            //TODO: ensure cart ItemID is proper for passing here
                            body.orderId = cartData.GetInfo().ItemID.ToString();
                            
                           
                            //make request for token
                            var client = GetWebClient(encodedString, "");
                            var requestSerializer = new DataContractJsonSerializer(typeof(TokenRequest));
                            var requestMs = new MemoryStream();
                            requestSerializer.WriteObject(requestMs, body);

                            var responseData = client.UploadData(_serviceUrl + "V4/Charge/CreatePayment", "POST", requestMs.ToArray());

                            var responseMs = new System.IO.MemoryStream(responseData);
                            var responseSerializer = new DataContractJsonSerializer(typeof(TokenResponse));
                            var tokenResponse = responseSerializer.ReadObject(responseMs) as TokenResponse;

                            if (tokenResponse != null)
                            {
                                if (tokenResponse.status == "SUCCESS")
                                {
                                    strOut = tokenResponse.answer.formToken;
                                }
                            }
                        }
                    }

                    break;
            }

            return strOut;

        }

        private System.Net.WebClient GetWebClient(string encodedString, string UAComment)
        {
            var webClient = new System.Net.WebClient();
            //TODO: Verify protocol compliancy
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            webClient.Headers["Content-type"] = "application/json";
            webClient.Headers["Authorization"] = "Basic " + encodedString;

            var av = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var version = av.Major + "." + av.Minor;
            var UserAgent = ("OS_Sog/" + version + " " + UAComment).Trim();

            webClient.Headers["User-Agent"] = UserAgent;

            return webClient;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public override void Validate()
        {
        }

    }
}
