## Tiup SSO Login SDK for CSharp

## Setup Project in Visual Studio

1.   Open the `scr/DotNetOpenAuth.TiupSso.sln` with Visual Studio.
2.   Click on `Build > Rebuild Solution`.
3.   The .dll is copied to `..\Bin`.

## Setup Your Project in Visual Studio In NuGet

1. Install JSON And DotNetOpenAuth

             Install-Package Newtonsoft.Json
             Install-Package DotNetOpenAuth.AspNet
2. Include the build dll in your project

3. Read the reference by your project is aspx or mvc

            https://github.com/DotNetOpenAuth/DotNetOpenAuth/wiki/Creating-an-openid-relying-party-%28programatically%29

## Below you can find code snippets whether you're using ASP.NET MVC or ASP.NET web forms. (From DotNetOpenAuth)
# MVC
#### The Sample Code is In ASP.NET MVC.

First step to create an OpenID logon for ASP.NET mvc is to implement a controller that controls the login flow. A sample of the login methods is provided below.

The LogOn method without any parameters is the action that will be invoked when the logon procedure starts. Because openID also returns to this action after logon we need to check if there is a response from the provider before continuing.

After the user has entered an OpenID url and clicks Logon in the view, the user is directed to the LogOn(String loginIdentifier) method which creates the actual logon request.

            using DotNetOpenAuth.Messaging;

            public ActionResult LogOn()
            {
                var openid = new OpenIdRelyingParty();
                IAuthenticationResponse response = openid.GetResponse();

                if (response != null)
                {
                    switch (response.Status)
                    {
                        case AuthenticationStatus.Authenticated:
                            FormsAuthentication.RedirectFromLoginPage(
                                response.ClaimedIdentifier, false);
                            break;
                        case AuthenticationStatus.Canceled:
                            ModelState.AddModelError("loginIdentifier",
                                "Login was cancelled at the provider");
                            break;
                        case AuthenticationStatus.Failed:
                            ModelState.AddModelError("loginIdentifier",
                                "Login failed using the provided OpenID identifier");
                            break;
                    }
                }

                return View();
            }

            [System.Web.Mvc.AcceptVerbs(HttpVerbs.Post)]
            public ActionResult LogOn(string loginIdentifier)
            {
                if (!Identifier.IsValid(loginIdentifier))
                {
                    ModelState.AddModelError("loginIdentifier",
                                "The specified login identifier is invalid");
                    return View();
                }
                else
                {
                    var openid = new OpenIdRelyingParty();
                    IAuthenticationRequest request = openid.CreateRequest(
                        Identifier.Parse(loginIdentifier));

                    // Require some additional data
                    request.AddExtension(new ClaimsRequest
                    {
                        BirthDate = DemandLevel.NoRequest,
                        Email = DemandLevel.Require,
                        FullName = DemandLevel.Require
                    });

                    return request.RedirectingResponse.AsActionResult();
                }
            }

# Web forms
If you're using ASP.NET web forms and just want to wire up the control yourself for maximum flexibility, there is an easy API for handling the OpenID authentication in your code behind or MVC controller. The layout is totally up to you, but the code-behind example that will follow will assume these controls exist:

             <asp:Label ID="Label1" runat="server" Text="OpenID Login" />
             <asp:TextBox ID="openIdBox" runat="server" />
             <asp:Button ID="loginButton" runat="server" Text="Login" OnClick="loginButton_Click" />
             <asp:CustomValidator runat="server" ID="openidValidator" ErrorMessage="Invalid OpenID Identifier"
                     ControlToValidate="openIdBox" EnableViewState="false" OnServerValidate="openidValidator_ServerValidate" />
             <br />
             <asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
                     Visible="False" />
             <asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False" Text="Login canceled"
                     Visible="False" />
First we can implement the custom validator that helps ensure the user typed in a meaningful form of an identifier

            protected void openidValidator_ServerValidate(object source, ServerValidateEventArgs args) {
                    // This catches common typos that result in an invalid OpenID Identifier.
                    args.IsValid = Identifier.IsValid(args.Value);
            }
Now we need to handle the Login button and cause the OpenID login to redirect the user to their Provider to complete authentication:

            protected void loginButton_Click(object sender, EventArgs e) {
                    if (!this.Page.IsValid) {
                            return; // don't login if custom validation failed.
                    }
                    try {
                            using (OpenIdRelyingParty openid = new OpenIdRelyingParty()) {
                                    IAuthenticationRequest request = openid.CreateRequest(this.openIdBox.Text);

                                    // This is where you would add any OpenID extensions you wanted
                                    // to include in the authentication request.
                                    request.AddExtension(new ClaimsRequest {
                                            Country = DemandLevel.Request,
                                            Email = DemandLevel.Request,
                                            Gender = DemandLevel.Require,
                                            PostalCode = DemandLevel.Require,
                                            TimeZone = DemandLevel.Require,
                                    });

                                    // Send your visitor to their Provider for authentication.
                                    request.RedirectToProvider();
                            }
                    } catch (ProtocolException ex) {
                            // The user probably entered an Identifier that
                            // was not a valid OpenID endpoint.
                            this.openidValidator.Text = ex.Message;
                            this.openidValidator.IsValid = false;
                    }
            }
Finally, when the redirect is complete the user will return to this same login page. So in its Load event handler you need to sniff the request for a completed OpenID login message:

            protected void Page_Load(object sender, EventArgs e) {
                    openIdBox.Focus();

                    OpenIdRelyingParty openid = new OpenIdRelyingParty();
                    var response = openid.GetResponse();
                    if (response != null) {
                            switch (response.Status) {
                                    case AuthenticationStatus.Authenticated:
                                            // This is where you would look for any OpenID extension responses included
                                            // in the authentication assertion.
                                            var claimsResponse = response.GetExtension<ClaimsResponse>();
                                            Database.ProfileFields = claimsResponse;

                                            // Store off the "friendly" username to display -- NOT for username lookup
                                            Database.FriendlyLoginName = response.FriendlyIdentifierForDisplay;

                                            // Use FormsAuthentication to tell ASP.NET that the user is now logged in,
                                            // with the OpenID Claimed Identifier as their username.
                                            FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, false);
                                            break;
                                    case AuthenticationStatus.Canceled:
                                            this.loginCanceledLabel.Visible = true;
                                            break;
                                    case AuthenticationStatus.Failed:
                                            this.loginFailedLabel.Visible = true;
                                            break;
                            }
                    }
            }




