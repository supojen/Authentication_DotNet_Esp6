# 預備知識

* <span style="font-weight:900;">session - cookie</span>

    ![image](/image/SessionCookie.png)


* <span style="font-weight:900;">Jwt Web Token</span>

    - 驗證過程
        1. 伺服器端在收到登入請求後驗證使用者
        1. 伺服器端產生和回傳一組帶有資訊，且僅能在伺服器端被驗證的 Token
        1. Token 被回傳後，存取在「客戶端」（大多存在瀏覽器的 Storage 當中）
        1. 往後客戶端向伺服器端發送請求時，皆附帶此 Token 讓伺服器端驗證
        1. 若伺服器端在請求中沒有找到 Token，回傳錯誤；若有找到 Token 則驗證

    - 示意圖
    
        ![image](/image/JWT.png)

    - 組成
        1. Header
            - 共含有兩種資訊, Token種類 & 雜湊演算法資訊
            - { "alg": "HS256", "typ": "JWT"}
        1. Payload
            - 這裡放聲明內容，可以說就是存放溝通訊息的地方，在定義上有 3 種聲明 (Claims)
                1. Reserved (註冊聲明)
                    1. iss (Issuer) - jwt簽發者
                    1. sub (Subject) - jwt所面向的用戶 **User ID**
                    1. aud (Audience) - 接收jwt的一方
                    1. exp (Expiration Time) - jwt的過期時間，這個過期時間必須要大於簽發時間
                    1. nbf (Not Before) - 定義在什麼時間之前，該jwt都是不可用的
                    1. iat (Issued At) - jwt的簽發時間
                    1. jti (JWT ID) - jwt的唯一身份標識，主要用來作為一次性token,從而迴避重放攻擊
                1. Public (公開聲明)
                1. Private (私有聲明)
        1. Signature
            - 透過被轉換成 Base64 的 Header & Payload 再加上秘鑰,經過 Header 提供的雜湊演算法 Hash 過後的來的簽名
            - 主要是要檢查 Header & Payload 是不是有被惡意更改
            - secret 要保存在 server 端，jwt 的 簽發驗證都必須使用這個 secret，當其他人得知這個 secret，那就意味著客戶端是可以自我簽發 jwt ，因此在任何場景都不應該外流

    - 基本使用方法
        ``` 
        post('api/user/1', {
            headers: {
                'Authorization': 'Bearer ' + token
            }
        })
        ```
    
* Jwt 編碼、簽名、加密 的釐清
    
    * 編碼小知識
        * 字元編碼（英語：Character encoding）、字集碼是把字元集中的字元編碼為指定集合中某一物件（例如：位元模式、自然數序列、8位元組或者電脈衝），以便文字在電腦中儲存和通過通信網路的傳遞。
        * UTF8 vs Base64
            * UTF-8（dao8-bit Unicode Transformation Format）是一種針對Unicode的可變長度字bai符編碼，又稱萬國碼
            * Base64是網絡上最常見的用於傳輸8Bit位元組代碼的編碼方式之一
        * 在 jwt 中以 . 分割的三個部分都經過 base64 編碼 (secret 部分是否進行 base64 編碼是可選的，header 和 payload 則是必須進行 base64 編碼)
    
    * 加密
        * 對稱式加密算法 (Symmetric cryptography)
            - 只有一把鑰匙
        * 非對稱式加密算法 (Asymmetric cryptography)
            - 有公鑰和私鑰
            - RSA
    
    * **RSA不僅可以拿來加密,也可用作簽名上面**
        - 加密
            - 只有我自己才能解密，所以 公鑰負責加密，私鑰負責解密 。這是大多數的使用場景，使用 rsa 來加密
        - 簽名
            - 只有我才能發佈簽名，所以 私鑰負責簽名，公鑰負責驗證 
        - **jwt 中並沒有純粹的加密過程，而是使加密之虛，行簽名之實**

<br><br>    

### 範例
___
* 此次範例中,加密名的算法是使用對稱式而非非對稱式,請注意.

* @@ 生產 Jwt Bearer token @@
    1. 安裝 Nuhget Package
        > Microsoft.AspNetCore.Authentication.JwtBearer
    1. 產生 Jwt Beaer 的過程
        1. 產生 claims
            ```c# 
            // 所有 Claim 當中 sub 指的是 client id, 一定要有, 很重要。
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,"some id"),
                new Claim("department","Technology"),
                new Claim("level","1")
            };
            ```
        1. 產生一組只有 server 擁有的鑰匙,並定義好加密演算法
            ```c#
            // 生產出一組能用在對稱式加密算法的鑰匙
            var secretBytes = Encoding.UTF8.GetBytes(JwtTokenConstants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);
            // 定義要使用哪一種對稱式加密算法
            var algorithm = SecurityAlgorithms.HmacSha256;
            // 定義好要怎麼產生 Jwt 的第三部分,簽名。
            var signinCredentials = new SigningCredentials(key,algorithm);
            ```
        1. 準備好 Jwt Bearer 需要用到的參數
            ```c#
            // 需要以下幾種參數
            //  1. Issuer           : 發行 token 的 server 的 url 
            //  2. Audience         : client 的 url
            //  3. Claims           : 裝載一些 Client 的訊息
            //  4. notBefore        : token 的起使日期
            //  5. expires          : token 的到期日期         
            //  6. SigninCredentials: Jwt 的第三部分
            var token = new JwtSecurityToken(
                JwtTokenConstants.Issuer,
                JwtTokenConstants.Audience,
                claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(30),
                signinCredentials
            );
            ```
        1. 產生 Jwt Bearer Token
            ```c# 
            var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);
            ```

* @@ 使 .Net Core 使用 Jwt bearer 做為 Authentication 的方式 @@ 
    ```c#
    services.AddAuthentication("OAuth")
            .AddJwtBearer("OAuth", config => {

                var secretBytes = Encoding.UTF8.GetBytes(JwtTokenConstants.Secret);
                var key = new SymmetricSecurityKey(secretBytes);

                // 設定一些檢查 Jwt Bearer 的參數
                // 檢查 Bearer token 是否合法
                //  1. Issuer url 是否正確
                //  2. Audience url 是否正確
                //  3. 用表留住的 Key 對 Jwt 第三部分進行簽名驗證
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = JwtTokenConstants.Issuer,
                    ValidAudience = JwtTokenConstants.Audience,
                    IssuerSigningKey = key
                };

                // 當要用 Jwt Bearer 進行 Authentication 時, 查看 Query String 裡是否有 Access Token
                config.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Query.ContainsKey("access_token"))
                            context.Token = context.Request.Query["access_token"];

                        return Task.CompletedTask;
                    }
                };

            });
    ```