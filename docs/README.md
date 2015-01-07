## Tiup SSO Login SDK for CSharp


Setup Project in Visual Studio
----------------------------

1.   Open the `TiupSsoAPI.sln` with Visual Studio.
2.   Click on `Build > Rebuild Solution`.
3.   The .dll is copied to `..\Bin`.

Samples
-----

After building TiupSsoAPI.sln as described above, feel free to peruse and run the examples provided in the `Samples` directory:

1.   Open the Samples.sln file in Visual Studio.
2.   Define the `TiupSsoAPI` and `TiupSsoAppSecret` attributes in the `Web.config` file with your Dwolla application key and secret respectively.
2.   Right-click on the web project labeled `Samples` and click `View in Browser`.

### 用户信息

* path: /user

    + Headers

             Authorization: <oauth 2.0 token>

    + User

            {
              "id": "121313131",
              "email": "leeannxi@gmail.com",
              "phone_number": "1212131312",
              "school_accounts": [
                {
                  "school_code": "121",
                  "school_name": "admin"
                },
                {
                  "school_code": "151",
                  "school_name": "user"
                }
              ]
            }


