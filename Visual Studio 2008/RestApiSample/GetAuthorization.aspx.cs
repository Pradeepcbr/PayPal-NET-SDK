﻿// #GetAuthorization Sample
// This sample code demonstrates how to 
// retrieve an Authorization resource
// API used: GET /v1/payments/authorization/{id}
using System;
using System.Web;
using PayPal.Api.Payments;
using PayPal;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RestApiSample
{
    public partial class GetAuthorization : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpContext CurrContext = HttpContext.Current;
            try
            {
                // ###AccessToken
                // Retrieve the access token from
                // OAuthTokenCredential by passing in
                // ClientID and ClientSecret
                // It is not mandatory to generate Access Token on a per call basis.
                // Typically the access token can be generated once and
                // reused within the expiry window
                string accessToken = new OAuthTokenCredential("EBWKjlELKMYqRNQ6sYvFo64FtaRLRR5BdHEESmha49TM", "EO422dn3gQLgDbuwqTjzrFgFtaRLRR5BdHEESmha49TM", Configuration.GetConfig()).GetAccessToken();

                // ### Api Context
                // Pass in a `ApiContext` object to authenticate 
                // the call and to send a unique request id 
                // (that ensures idempotency). The SDK generates
                // a request id if you do not pass one explicitly. 
                APIContext context = new APIContext(accessToken);
                context.Config = Configuration.GetConfig();
                   
                // ###Authorization
                // Retrieve an Authorization Id
                // by making a Payment with intent
                // as 'authorize' and parsing through
                // the Payment object
                string authorizationId = GetAuthorizationId(accessToken);
                
                // Get Authorization by sending
                // a GET request with authorization Id
                // to the
                // URI v1/payments/authorization/{id}
                Authorization authorization = Authorization.Get(context, authorizationId);
                CurrContext.Items.Add("ResponseJson", JObject.Parse(authorization.ConvertToJson()).ToString(Formatting.Indented));
            }
            catch (PayPal.Exception.PayPalException ex)
            {
                CurrContext.Items.Add("Error", ex.Message);
            }

            Server.Transfer("~/Response.aspx");
        }

        private string GetAuthorizationId(string accessToken)
        {
            // ###Address
            // Base Address object used as shipping or billing
            // address in a payment.
            Address billingAddress = new Address();
            billingAddress.city = "Johnstown";
            billingAddress.country_code = "US";
            billingAddress.line1 = "52 N Main ST";
            billingAddress.postal_code = "43210";
            billingAddress.state = "OH";

            // ###CreditCard
            // A resource representing a credit card that can be
            // used to fund a payment.
            CreditCard crdtCard = new CreditCard();
            crdtCard.billing_address = billingAddress;
            crdtCard.cvv2 = "874";
            crdtCard.expire_month = 11;
            crdtCard.expire_year = 2018;
            crdtCard.first_name = "Joe";
            crdtCard.last_name = "Shopper";
            crdtCard.number = "4417119669820331";
            crdtCard.type = "visa";

            // ###Details
            // Let's you specify details of a payment amount.
            Details details = new Details();
            details.shipping = "0.03";
            details.subtotal = "107.41";
            details.tax = "0.03";

            // ###Amount
            // Let's you specify a payment amount.
            Amount amnt = new Amount();
            amnt.currency = "USD";
            // Total must be equal to sum of shipping, tax and subtotal.
            amnt.total = "107.47";
            amnt.details = details;

            // ###Transaction
            // A transaction defines the contract of a
            // payment - what is the payment for and who
            // is fulfilling it. Transaction is created with
            // a `Payee` and `Amount` types
            Transaction tran = new Transaction();
            tran.amount = amnt;
            tran.description = "This is the payment transaction description.";

            // The Payment creation API requires a list of
            // Transaction; add the created `Transaction`
            // to a List
            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            // ###FundingInstrument
            // A resource representing a Payeer's funding instrument.
            // Use a Payer ID (A unique identifier of the payer generated
            // and provided by the facilitator. This is required when
            // creating or using a tokenized funding instrument)
            // and the `CreditCardDetails`
            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;

            // The Payment creation API requires a list of
            // FundingInstrument; add the created `FundingInstrument`
            // to a List
            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);

            // ###Payer
            // A resource representing a Payer that funds a payment
            // Use the List of `FundingInstrument` and the Payment Method
            // as 'credit_card'
            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";

            // ###Payment
            // A Payment Resource; create one using
            // the above types and intent as `sale`
            Payment pymnt = new Payment();
            pymnt.intent = "authorize";
            pymnt.payer = payr;
            pymnt.transactions = transactions;

            // Create a payment by posting to the APIService
            // using a valid AccessToken
            // The return object contains the status;
            Payment createdPayment = pymnt.Create(accessToken);

            return createdPayment.transactions[0].related_resources[0].authorization.id;
        }
    }
}
