using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using OS_Sog.Models;
using System;
using System.Linq;

namespace OS_Sog
{
    public class ProviderUtils
    {
        private static readonly string _locationId;
        //private static readonly SogClient _client;
        private static readonly NBrightInfo _settings;

        static ProviderUtils() {
            _settings = ProviderUtils.GetProviderSettings();
            var accessToken = NBrightCore.common.Security.Decrypt(PortalController.Instance.GetCurrentSettings().GUID.ToString(), _settings.GetXmlProperty("genxml/textbox/accesstoken"));
            var sandboxMode = _settings.GetXmlPropertyBool("genxml/checkbox/sandboxmode");
            
        }

        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_Sogpayment", "OS_SogPAYMENT", Utils.GetCurrentCulture());
            return info;
        }
        
        public static PaymentResponse GetChargeResponse(OrderData orderData, string nonce)
        {

            return new PaymentResponse();
        }

    }
}
