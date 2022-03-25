using NBrightBuy.render;
using NBrightDNN;
using RazorEngine.Text;

namespace OS_Sog.render
{
    public class RazorTokens<T> : NBrightBuyRazorTokens<T>
    {

        public IEncodedString InvoiceButton(NBrightInfo info)
        {
            return new RawString("<button class='btn btn-secondary' id='squareInvoice'><i class='fas fa-square'></i> Send Square Invoice</button>");
        }


    }
}
