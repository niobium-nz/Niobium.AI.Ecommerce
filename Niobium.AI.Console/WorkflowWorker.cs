using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Niobium.AI.Console
{
    internal class WorkflowWorker(IWorkflow workflow, ILogger<WorkflowWorker> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string rendering = workflow.Render();
            logger.LogInformation($"Running workflow: \n{rendering}");

            Guid conversationID = Guid.NewGuid();
            //string input = """
            //    {
            //        "BusinessName": "Mid-Class Community Restaurant and Bar",
            //        "Location": "Morningside, Auckland, New Zealand",
            //        "BusinessType": "Restaurant and Bar",
            //        "ProductsSold": [
            //            "Food",
            //            "Alcoholic Beverages",
            //            "Non-Alcoholic Beverages"
            //        ],
            //        "TypicalSpend": "$20-$50 per person",
            //        "AdAccountId": "1995422867683456",
            //        "CampaignName": "Followers",
            //        "AdSetName": "Attractive Shorts"
            //    }
            //    """;

            //string input = """
            //    {
            //        "CategoryFocus": "pet grooming",
            //        "SourceCountry": "US",
            //        "TargetCountry": "AU"
            //    }
            //    """;

            //string input = """
            //    {"SourceCountry":"US","TargetCountry":"AU","Keyword":"pet grooming supplies"}
            //    """;

            //string input = """
            //                    {
            //      "Product": {
            //        "Product": {
            //          "ClusterId": "cl_magicbrushofficial_com_magic_brush",
            //          "ClusterLabel": "The Magic Brush",
            //          "LandingPageDomain": "magicbrushofficial.com",
            //          "LikelyProductName": "The Magic Brush",
            //          "CategoryGuess": "Pet hair remover brush/glove",
            //          "KnownFeatures": [ "soft brush cloth bristles generate static", "removes pet hair from furniture, clothing, and car interiors", "adjustable wrist strap", "reusable and easy to clean", "buy 1 get 1 free offer" ],
            //          "ClusterConfidence": "High",
            //          "AdArchiveIds": [ "1211931013961670", "1280599013387623", "1673990680285512", "4771378853182285" ]
            //        },
            //        "Ads": [
            //          {
            //            "AdArchiveId": "1280599013387623",
            //            "CollationId": "1117274810507775",
            //            "PageId": "294643010396342",
            //            "Snapshot": {
            //              "PageId": "294643010396342",
            //              "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //              "PageName": "The Magic Brush",
            //              "PageProfilePictureUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/545586119_793033416590173_2197844232583729508_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=_qM5BrDVXjQQ7kNvwH_G_Cm\u0026_nc_oc=AdmqDPCE1aOGI6Z7tQKv9Q8L0WjFQK4ptfu76S_e4F9wBpsZRB1EhKCQjxl9rnt14Xo\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_Aft_5UAWTd5yBrhKwdoemK0t8rtHUs1wuzkHUv1yae3Qbg\u0026oe=699B0699",
            //              "DisplayFormat": "VIDEO",
            //              "PageCategories": [ "Pet Store" ],
            //              "PageLikeCount": 7477,
            //              "IsReshared": false,
            //              "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //              "CtaType": "SHOP_NOW",
            //              "CtaText": "Shop now",
            //              "Caption": "magicbrushofficial.com",
            //              "LinkDescription": null,
            //              "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //              "Title": "\uD83D\uDEA8 BUY 1 GET 1 ENDS TODAY! \uD83D\uDEA8",
            //              "OriginalImageUrl": null,
            //              "ResizedImageUrl": null,
            //              "Images": [],
            //              "Videos": [
            //                {
            //                  "VideoHdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQPrgttQ4KVfF3age_rdVNsVhHIdBrTk5y_VvqEq4JnnL3DDVuOSSuO2-2iy6GYi9j-SdT1n2LPzFLI2zWBf173M42BC5j0AP1_gg41ziRZK6A.mp4?_nc_cat=102\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=k6fDRFmIj9cQ7kNvwHyinFq\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjExMTMzMjE4NzQyMzE0ODYsImFzc2V0X2FnZV9kYXlzIjoxNjAsInZpX3VzZWNhc2VfaWQiOjEwNzk5LCJkdXJhdGlvbl9zIjoxNiwidXJsZ2VuX3NvdXJjZSI6Ind3dyJ9\u0026ccb=17-1\u0026vs=57ab526d01523d13\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9CQjQwRDJBNzdDRTU0QUI1N0ZBOTc4QjZEMUQxMUNBM19tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50L0IyNDAzMzlCMDRGREZBOEU2OEI0QkJCN0VFNDRGQjhDX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACb8wqyt7qP6AxUCKAJDMywXQDDzMzMzMzMYGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZd6oAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_Afu_Jkh5u3BE9NgalZgo2BhG_4WLMbUm3_4VbCqGoend2w\u0026oe=699B2E3E",
            //                  "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/545665919_1185840076755886_3928460495946496098_n.jpg?_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=oEJfbgSS_-8Q7kNvwGnjq79\u0026_nc_oc=Adk4a4Bl0RvVhgOQmjKmQuQ0oC7fLdosH0a-aF2S-HDHkppXtcjF-pM9HsdWwYFFlLo\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfvFu_gatC3S1i4e8IZ0wXsrpSK2jNfke0QKpUNoX78XHQ\u0026oe=699B0A25",
            //                  "VideoSdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQP1atvb7jwmUbDsWgQvMc5eaJYX_bR6b01pM-I0Rsws8NF0JGVLCWMvTnS49cmQ_qccU1xlRYWTOG2hK7RvcHk9AAhSqatEdqFNusMehw.mp4?_nc_cat=103\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=mz7PVnZFpd8Q7kNvwH1nwvi\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTExMzMyMTg3NDIzMTQ4NiwiYXNzZXRfYWdlX2RheXMiOjE2MCwidmlfdXNlY2FzZV9pZCI6MTA3OTksImR1cmF0aW9uX3MiOjE2LCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfvHQgnXo_qYPmd5MNnJyTHb0Qz9uPExLrzNkX_592XGLA\u0026oe=699B2217"
            //                }
            //              ],
            //              "Cards": []
            //            },
            //            "IsActive": true,
            //            "HasUserReported": false,
            //            "PageIsDeleted": false,
            //            "PageName": "The Magic Brush",
            //            "Categories": [ "UNKNOWN" ],
            //            "ContainsDigitalCreatedMedia": false,
            //            "EndDate": 1771315200,
            //            "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //            "StartDate": 1757487600,
            //            "ContainsSensitiveContent": false,
            //            "Url": "https://www.facebook.com/ads/library?id=1280599013387623",
            //            "StartDateString": "2025-09-10T07:00:00.000Z",
            //            "EndDateString": "2026-02-17T08:00:00.000Z"
            //          },
            //          {
            //            "AdArchiveId": "1673990680285512",
            //            "CollationId": "1117274810507775",
            //            "PageId": "294643010396342",
            //            "Snapshot": {
            //              "PageId": "294643010396342",
            //              "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //              "PageName": "The Magic Brush",
            //              "PageProfilePictureUrl": "https://scontent-sjc6-1.xx.fbcdn.net/v/t39.35426-6/634760428_884386727853009_8714410255500028444_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=107\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=uRwIyJxmWsIQ7kNvwE4Tl-Z\u0026_nc_oc=Adl0-47XBdSH-eYEfwRCfC5BXqv5tipI8HIBfVe7Et9SpE0duz_W6jZQZEGzPPqhOl8\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc6-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_Afsm3bTe1PRJziG1wsUCAc0BkLGnNwf640fKFICpY3zT-w\u0026oe=699B203F",
            //              "DisplayFormat": "VIDEO",
            //              "PageCategories": [ "Pet Store" ],
            //              "PageLikeCount": 7477,
            //              "IsReshared": false,
            //              "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //              "CtaType": "SHOP_NOW",
            //              "CtaText": "Shop now",
            //              "Caption": "magicbrushofficial.com",
            //              "LinkDescription": null,
            //              "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //              "Title": "\uD83D\uDEA8 BUY 1 GET 1 ENDS TODAY! \uD83D\uDEA8",
            //              "OriginalImageUrl": null,
            //              "ResizedImageUrl": null,
            //              "Images": [],
            //              "Videos": [
            //                {
            //                  "VideoHdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQPrgttQ4KVfF3age_rdVNsVhHIdBrTk5y_VvqEq4JnnL3DDVuOSSuO2-2iy6GYi9j-SdT1n2LPzFLI2zWBf173M42BC5j0AP1_gg41ziRZK6A.mp4?_nc_cat=102\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=k6fDRFmIj9cQ7kNvwHyinFq\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjExMTMzMjE4NzQyMzE0ODYsImFzc2V0X2FnZV9kYXlzIjoxNjAsInZpX3VzZWNhc2VfaWQiOjEwNzk5LCJkdXJhdGlvbl9zIjoxNiwidXJsZ2VuX3NvdXJjZSI6Ind3dyJ9\u0026ccb=17-1\u0026vs=57ab526d01523d13\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9CQjQwRDJBNzdDRTU0QUI1N0ZBOTc4QjZEMUQxMUNBM19tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50L0IyNDAzMzlCMDRGREZBOEU2OEI0QkJCN0VFNDRGQjhDX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACb8wqyt7qP6AxUCKAJDMywXQDDzMzMzMzMYGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZd6oAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_Afu_Jkh5u3BE9NgalZgo2BhG_4WLMbUm3_4VbCqGoend2w\u0026oe=699B2E3E",
            //                  "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/634029413_1568991937511684_4425860879435828882_n.jpg?_nc_cat=100\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=bxVvboXhfvAQ7kNvwFQPLje\u0026_nc_oc=AdnQz9vSFYwQiYmOEoNV_zBWk6clY8me1Wm4qI5jQAVmMmPkx3Szd6EtK7rJ9ilfWuw\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_Aftn-GfglDlRBog67h7yYusZ0uci87JaB4bMTdD3jxMMaA\u0026oe=699B22AD",
            //                  "VideoSdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQP1atvb7jwmUbDsWgQvMc5eaJYX_bR6b01pM-I0Rsws8NF0JGVLCWMvTnS49cmQ_qccU1xlRYWTOG2hK7RvcHk9AAhSqatEdqFNusMehw.mp4?_nc_cat=103\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=mz7PVnZFpd8Q7kNvwH1nwvi\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTExMzMyMTg3NDIzMTQ4NiwiYXNzZXRfYWdlX2RheXMiOjE2MCwidmlfdXNlY2FzZV9pZCI6MTA3OTksImR1cmF0aW9uX3MiOjE2LCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfvHQgnXo_qYPmd5MNnJyTHb0Qz9uPExLrzNkX_592XGLA\u0026oe=699B2217"
            //                }
            //              ],
            //              "Cards": []
            //            },
            //            "IsActive": true,
            //            "HasUserReported": false,
            //            "PageIsDeleted": false,
            //            "PageName": "The Magic Brush",
            //            "Categories": [ "UNKNOWN" ],
            //            "ContainsDigitalCreatedMedia": false,
            //            "EndDate": 1771315200,
            //            "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //            "StartDate": 1770969600,
            //            "ContainsSensitiveContent": false,
            //            "Url": "https://www.facebook.com/ads/library?id=1673990680285512",
            //            "StartDateString": "2026-02-13T08:00:00.000Z",
            //            "EndDateString": "2026-02-17T08:00:00.000Z"
            //          },
            //          {
            //            "AdArchiveId": "1211931013961670",
            //            "CollationId": "1104694201152610",
            //            "PageId": "294643010396342",
            //            "Snapshot": {
            //              "PageId": "294643010396342",
            //              "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //              "PageName": "The Magic Brush",
            //              "PageProfilePictureUrl": "https://scontent-sjc6-1.xx.fbcdn.net/v/t39.35426-6/537396336_4053792711434136_8700917304993101717_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=107\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=zQC3qoX_DW8Q7kNvwF85ZHx\u0026_nc_oc=AdkJnzcfDZXlHjrQ1syZgd7tnu5myAun8Nx1fKQ9jSqBKc0-NaZXeTE0faz-HlJd9kM\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc6-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AftpKB_34RDTvRnglWnyXhn7rYja7GvcxnSX5lQUn36p2w\u0026oe=699B0236",
            //              "DisplayFormat": "VIDEO",
            //              "PageCategories": [ "Pet Store" ],
            //              "PageLikeCount": 7477,
            //              "IsReshared": false,
            //              "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //              "CtaType": "SHOP_NOW",
            //              "CtaText": "Shop now",
            //              "Caption": "magicbrushofficial.com",
            //              "LinkDescription": null,
            //              "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //              "Title": "\uD83D\uDEA8 BUY 1 GET 1 ENDS TODAY! \uD83D\uDEA8",
            //              "OriginalImageUrl": null,
            //              "ResizedImageUrl": null,
            //              "Images": [],
            //              "Videos": [
            //                {
            //                  "VideoHdUrl": "https://video-sjc6-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQPbr-TbaFG6ipGlyt6AjtM26paf0_feOvHQSeufBKki5U5V9QqUMzFnNjCl9xzKFH-6jc4prbCjaF_pxMkFg5Zc6Q-a15xOEOG3MKuIRlJOsg.mp4?_nc_cat=104\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc6-1.xx.fbcdn.net\u0026_nc_ohc=j4yWbfh6NTUQ7kNvwGep3m1\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjE3NzE4OTA4NzY3NjE0NzMsImFzc2V0X2FnZV9kYXlzIjoxNzksInZpX3VzZWNhc2VfaWQiOjEwNzk5LCJkdXJhdGlvbl9zIjo1MSwidXJsZ2VuX3NvdXJjZSI6Ind3dyJ9\u0026ccb=17-1\u0026vs=b22c89786956ab11\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9EQjRDNTVGQ0RERUE4Q0VEODM5QThDRkNDMjA4QzdBNl9tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50LzhGNEEyRUIwNTBENTdBMjhEMTc5RUM0RjdERjM0OThCX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACaCltLkz-GlBhUCKAJDMywXQEmEOVgQYk4YGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZd6oAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfsAld6l5Yt5y4ds1UUhFjzSSV9brZDOlByrj-tQWUuMZQ\u0026oe=699B21F0",
            //                  "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/537478688_2569965090037732_819948677684452553_n.jpg?_nc_cat=100\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=4fsS7z4eclwQ7kNvwFWQdGg\u0026_nc_oc=AdnXYZqsm8kI2q6fG2D0FmdZj7g_M2aRfn6eakSSxDoE_lumLQlXSFPCjldnWeguqYI\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfszrjhBe3xbT_Gzi5Kc5qs5o1Uu4qoMmV4OnZ1Jme731w\u0026oe=699B30C2",
            //                  "VideoSdUrl": "https://video-sjc6-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQNIY7GpY1usjQdq9iqbxvJ8s_YlR5cgPAhURTAx6zW-36Q2-H9NVSLmgpYef-TLJ7d4XdBkmTvV4wgNnmFcamDm7X3B0Q8fPftIV3lTNQ.mp4?_nc_cat=104\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc6-1.xx.fbcdn.net\u0026_nc_ohc=pg_XB5y-xm4Q7kNvwHsg3XF\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTc3MTg5MDg3Njc2MTQ3MywiYXNzZXRfYWdlX2RheXMiOjE3OSwidmlfdXNlY2FzZV9pZCI6MTA3OTksImR1cmF0aW9uX3MiOjUxLCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfsKYFqf91iXFLAvxhJBSLEA6Hhz-rGdEFmsl1_a574oRw\u0026oe=699B148A"
            //                }
            //              ],
            //              "Cards": []
            //            },
            //            "IsActive": true,
            //            "HasUserReported": false,
            //            "PageIsDeleted": false,
            //            "PageName": "The Magic Brush",
            //            "Categories": [ "UNKNOWN" ],
            //            "ContainsDigitalCreatedMedia": false,
            //            "EndDate": 1771315200,
            //            "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //            "StartDate": 1755932400,
            //            "ContainsSensitiveContent": false,
            //            "Url": "https://www.facebook.com/ads/library?id=1211931013961670",
            //            "StartDateString": "2025-08-23T07:00:00.000Z",
            //            "EndDateString": "2026-02-17T08:00:00.000Z"
            //          },
            //          {
            //            "AdArchiveId": "4771378853182285",
            //            "CollationId": "1134436051988038",
            //            "PageId": "294643010396342",
            //            "Snapshot": {
            //              "PageId": "294643010396342",
            //              "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //              "PageName": "The Magic Brush",
            //              "PageProfilePictureUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/629733920_4306473782974582_5377999549716268893_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=106\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=3_VH8y8b8P4Q7kNvwHoZcZB\u0026_nc_oc=AdkBKwDJFmSbll76ncp_2ZX9M3l_Nax084devNhxAd4XZYfB4Z7hkvE9J2tIZ3EXty4\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfsB0WKMJfrXU1NqXwVJE3_XWK-7dZDgPEDo4XzMmxHfwA\u0026oe=699B2A79",
            //              "DisplayFormat": "VIDEO",
            //              "PageCategories": [ "Pet Store" ],
            //              "PageLikeCount": 7477,
            //              "IsReshared": false,
            //              "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //              "CtaType": "SHOP_NOW",
            //              "CtaText": "Shop now",
            //              "Caption": "magicbrushofficial.com",
            //              "LinkDescription": null,
            //              "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //              "Title": "We Can\u0027t Believe How Viral our Magic Brush Has Gone!",
            //              "OriginalImageUrl": null,
            //              "ResizedImageUrl": null,
            //              "Images": [],
            //              "Videos": [
            //                {
            //                  "VideoHdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQN4qNURaFa-mJabqNmHO79E0J2bNcuCXd9j3EYDpWBttAIN1O1fryOtskBBYNiYbyX6Lp5-SJyStp0lwYFmvMBTbFgP2ny4LWhO_v8phrIA3w.mp4?_nc_cat=105\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=nQocJNzbJ6sQ7kNvwGpMlkX\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjEyMzI4MzA0NDkwNTgwMDMsImFzc2V0X2FnZV9kYXlzIjoxMSwidmlfdXNlY2FzZV9pZCI6MTAxMzksImR1cmF0aW9uX3MiOjQ5LCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026vs=e1a3f97fee3f640c\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9CRjRBRDIwQzAxODFCNkIzQ0M1Q0ExRkNCQUM4ODc4Nl9tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50LzIwNDE5Q0NENjM0NTkzMkYxMERCMzk5QkU4M0Q3QjgzX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACamg-DPltCwBBUCKAJDMywXQEjZmZmZmZoYGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZbaeAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_Afv3bDMiZpVqdGwyk9Zhhuzhfduz1tYV1DYgsZlJC41DGw\u0026oe=699B11DB",
            //                  "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/628418219_905930095353371_1130105990814021226_n.jpg?_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=YcTnwXJ40NQQ7kNvwFkwLNd\u0026_nc_oc=Adn6BoZCZGGflYO-8q1P7MUck3Y7LC7TPojeuMzJaQ_pVJmlsiHlDZ_OijQ07-ppCYQ\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfuEzVaFXZesfqlI4fDbvcYJMVMOAYafQceJLur6derBDQ\u0026oe=699B2C72",
            //                  "VideoSdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m412/AQOuzepP0BwhaaOYFm-Wb-WrameZ2YK_dmDqrsxkkHIqS491bNNCKeUy0KCcq4O7-WwOjar6vEL09u-AJf0RJ40QqvV8LzX01fvol7IdEg.mp4?_nc_cat=103\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=CT46unDTBigQ7kNvwF8B48r\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTIzMjgzMDQ0OTA1ODAwMywiYXNzZXRfYWdlX2RheXMiOjExLCJ2aV91c2VjYXNlX2lkIjoxMDEzOSwiZHVyYXRpb25fcyI6NDksInVybGdlbl9zb3VyY2UiOiJ3d3cifQ%3D%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AftZ6uzYI7YAXLE5gtpt9pdON2kFzWiJjoNoW-HgAeFrgQ\u0026oe=699AFCBF"
            //                }
            //              ],
            //              "Cards": []
            //            },
            //            "IsActive": true,
            //            "HasUserReported": false,
            //            "PageIsDeleted": false,
            //            "PageName": "The Magic Brush",
            //            "Categories": [ "UNKNOWN" ],
            //            "ContainsDigitalCreatedMedia": false,
            //            "EndDate": 1771315200,
            //            "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //            "StartDate": 1770364800,
            //            "ContainsSensitiveContent": false,
            //            "Url": "https://www.facebook.com/ads/library?id=4771378853182285",
            //            "StartDateString": "2026-02-06T08:00:00.000Z",
            //            "EndDateString": "2026-02-17T08:00:00.000Z"
            //          }
            //        ]
            //      },
            //      "SourceCountry": "US",
            //      "TargetCountry": "AU",
            //      "Keyword": "pet grooming supplies"
            //    }
            //    """;

            //string input = """
            //{
            //  "NormalizedKeyword": "pet hair remover glove\u0022 furniture static",
            //  "CompetitorAnalysisNotes": [ "Keep the competitive set limited to hand-operated brush/glove surface-hair removers.", "Do not benchmark against on-pet grooming/deshedding tools; the stated job is cleaning furniture, clothing, and car interiors.", "Do not benchmark against vacuums or adhesive lint rollers unless intentionally exploring substitutes.", "Maintain form-factor tokens like glove or brush in Tier A/B to prevent broad category drift.", "Use static as a qualifier only because it was explicitly provided.", "Ignore the buy-1-get-1-free offer during competitive-set matching; it is promotional, not archetype-defining." ],
            //  "AvoidOrExclusionTerms": [ "vacuum pet hair remover", "pet hair roller", "lint roller", "grooming glove", "deshedding glove", "dog grooming brush", "cat grooming brush", "slicker brush", "carpet rake", "pet hair squeegee" ],
            //  "ProductInterpretations": [
            //    {
            //      "InterpretedProductType": "pet hair remover glove",
            //      "InterpretedArchetype": "static cloth pet hair removal glove for furniture and clothing",
            //      "WhyThisInterpretation": [ "The category explicitly includes glove.", "An adjustable wrist strap fits a wearable glove/mitt-like form factor.", "The stated job is surface hair removal rather than grooming the pet." ],
            //      "Confidence": "medium"
            //    },
            //    {
            //      "InterpretedProductType": "pet hair remover brush",
            //      "InterpretedArchetype": "static cloth pet hair removal brush for furniture and clothing",
            //      "WhyThisInterpretation": [ "The product name contains brush.", "The category explicitly includes brush.", "Soft brush cloth bristles support a brush-style interpretation." ],
            //      "Confidence": "medium"
            //    }
            //  ],
            //  "SourceCountry": "US",
            //  "TargetCountry": "AU",
            //  "Keyword": "pet grooming supplies",
            //  "Product": {
            //    "Product": {
            //      "ClusterId": "cl_magicbrushofficial_com_magic_brush",
            //      "ClusterLabel": "The Magic Brush",
            //      "LandingPageDomain": "magicbrushofficial.com",
            //      "LikelyProductName": "The Magic Brush",
            //      "CategoryGuess": "Pet hair remover brush/glove",
            //      "KnownFeatures": [ "soft brush cloth bristles generate static", "removes pet hair from furniture, clothing, and car interiors", "adjustable wrist strap", "reusable and easy to clean", "buy 1 get 1 free offer" ],
            //      "ClusterConfidence": "High",
            //      "AdArchiveIds": [ "1211931013961670", "1280599013387623", "1673990680285512", "4771378853182285" ]
            //    },
            //    "Ads": [
            //      {
            //        "AdArchiveId": "1280599013387623",
            //        "CollationId": "1117274810507775",
            //        "PageId": "294643010396342",
            //        "Snapshot": {
            //          "PageId": "294643010396342",
            //          "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //          "PageName": "The Magic Brush",
            //          "PageProfilePictureUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/545586119_793033416590173_2197844232583729508_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=_qM5BrDVXjQQ7kNvwH_G_Cm\u0026_nc_oc=AdmqDPCE1aOGI6Z7tQKv9Q8L0WjFQK4ptfu76S_e4F9wBpsZRB1EhKCQjxl9rnt14Xo\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_Aft_5UAWTd5yBrhKwdoemK0t8rtHUs1wuzkHUv1yae3Qbg\u0026oe=699B0699",
            //          "DisplayFormat": "VIDEO",
            //          "PageCategories": [ "Pet Store" ],
            //          "PageLikeCount": 7477,
            //          "IsReshared": false,
            //          "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //          "CtaType": "SHOP_NOW",
            //          "CtaText": "Shop now",
            //          "Caption": "magicbrushofficial.com",
            //          "LinkDescription": null,
            //          "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //          "Title": "\uD83D\uDEA8 BUY 1 GET 1 ENDS TODAY! \uD83D\uDEA8",
            //          "OriginalImageUrl": null,
            //          "ResizedImageUrl": null,
            //          "Images": [],
            //          "Videos": [
            //            {
            //              "VideoHdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQPrgttQ4KVfF3age_rdVNsVhHIdBrTk5y_VvqEq4JnnL3DDVuOSSuO2-2iy6GYi9j-SdT1n2LPzFLI2zWBf173M42BC5j0AP1_gg41ziRZK6A.mp4?_nc_cat=102\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=k6fDRFmIj9cQ7kNvwHyinFq\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjExMTMzMjE4NzQyMzE0ODYsImFzc2V0X2FnZV9kYXlzIjoxNjAsInZpX3VzZWNhc2VfaWQiOjEwNzk5LCJkdXJhdGlvbl9zIjoxNiwidXJsZ2VuX3NvdXJjZSI6Ind3dyJ9\u0026ccb=17-1\u0026vs=57ab526d01523d13\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9CQjQwRDJBNzdDRTU0QUI1N0ZBOTc4QjZEMUQxMUNBM19tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50L0IyNDAzMzlCMDRGREZBOEU2OEI0QkJCN0VFNDRGQjhDX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACb8wqyt7qP6AxUCKAJDMywXQDDzMzMzMzMYGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZd6oAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_Afu_Jkh5u3BE9NgalZgo2BhG_4WLMbUm3_4VbCqGoend2w\u0026oe=699B2E3E",
            //              "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/545665919_1185840076755886_3928460495946496098_n.jpg?_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=oEJfbgSS_-8Q7kNvwGnjq79\u0026_nc_oc=Adk4a4Bl0RvVhgOQmjKmQuQ0oC7fLdosH0a-aF2S-HDHkppXtcjF-pM9HsdWwYFFlLo\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfvFu_gatC3S1i4e8IZ0wXsrpSK2jNfke0QKpUNoX78XHQ\u0026oe=699B0A25",
            //              "VideoSdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQP1atvb7jwmUbDsWgQvMc5eaJYX_bR6b01pM-I0Rsws8NF0JGVLCWMvTnS49cmQ_qccU1xlRYWTOG2hK7RvcHk9AAhSqatEdqFNusMehw.mp4?_nc_cat=103\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=mz7PVnZFpd8Q7kNvwH1nwvi\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTExMzMyMTg3NDIzMTQ4NiwiYXNzZXRfYWdlX2RheXMiOjE2MCwidmlfdXNlY2FzZV9pZCI6MTA3OTksImR1cmF0aW9uX3MiOjE2LCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfvHQgnXo_qYPmd5MNnJyTHb0Qz9uPExLrzNkX_592XGLA\u0026oe=699B2217"
            //            }
            //          ],
            //          "Cards": []
            //        },
            //        "IsActive": true,
            //        "HasUserReported": false,
            //        "PageIsDeleted": false,
            //        "PageName": "The Magic Brush",
            //        "Categories": [ "UNKNOWN" ],
            //        "ContainsDigitalCreatedMedia": false,
            //        "EndDate": 1771315200,
            //        "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //        "StartDate": 1757487600,
            //        "ContainsSensitiveContent": false,
            //        "Url": "https://www.facebook.com/ads/library?id=1280599013387623",
            //        "StartDateString": "2025-09-10T07:00:00.000Z",
            //        "EndDateString": "2026-02-17T08:00:00.000Z"
            //      },
            //      {
            //        "AdArchiveId": "1673990680285512",
            //        "CollationId": "1117274810507775",
            //        "PageId": "294643010396342",
            //        "Snapshot": {
            //          "PageId": "294643010396342",
            //          "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //          "PageName": "The Magic Brush",
            //          "PageProfilePictureUrl": "https://scontent-sjc6-1.xx.fbcdn.net/v/t39.35426-6/634760428_884386727853009_8714410255500028444_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=107\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=uRwIyJxmWsIQ7kNvwE4Tl-Z\u0026_nc_oc=Adl0-47XBdSH-eYEfwRCfC5BXqv5tipI8HIBfVe7Et9SpE0duz_W6jZQZEGzPPqhOl8\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc6-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_Afsm3bTe1PRJziG1wsUCAc0BkLGnNwf640fKFICpY3zT-w\u0026oe=699B203F",
            //          "DisplayFormat": "VIDEO",
            //          "PageCategories": [ "Pet Store" ],
            //          "PageLikeCount": 7477,
            //          "IsReshared": false,
            //          "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //          "CtaType": "SHOP_NOW",
            //          "CtaText": "Shop now",
            //          "Caption": "magicbrushofficial.com",
            //          "LinkDescription": null,
            //          "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //          "Title": "\uD83D\uDEA8 BUY 1 GET 1 ENDS TODAY! \uD83D\uDEA8",
            //          "OriginalImageUrl": null,
            //          "ResizedImageUrl": null,
            //          "Images": [],
            //          "Videos": [
            //            {
            //              "VideoHdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQPrgttQ4KVfF3age_rdVNsVhHIdBrTk5y_VvqEq4JnnL3DDVuOSSuO2-2iy6GYi9j-SdT1n2LPzFLI2zWBf173M42BC5j0AP1_gg41ziRZK6A.mp4?_nc_cat=102\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=k6fDRFmIj9cQ7kNvwHyinFq\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjExMTMzMjE4NzQyMzE0ODYsImFzc2V0X2FnZV9kYXlzIjoxNjAsInZpX3VzZWNhc2VfaWQiOjEwNzk5LCJkdXJhdGlvbl9zIjoxNiwidXJsZ2VuX3NvdXJjZSI6Ind3dyJ9\u0026ccb=17-1\u0026vs=57ab526d01523d13\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9CQjQwRDJBNzdDRTU0QUI1N0ZBOTc4QjZEMUQxMUNBM19tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50L0IyNDAzMzlCMDRGREZBOEU2OEI0QkJCN0VFNDRGQjhDX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACb8wqyt7qP6AxUCKAJDMywXQDDzMzMzMzMYGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZd6oAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_Afu_Jkh5u3BE9NgalZgo2BhG_4WLMbUm3_4VbCqGoend2w\u0026oe=699B2E3E",
            //              "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/634029413_1568991937511684_4425860879435828882_n.jpg?_nc_cat=100\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=bxVvboXhfvAQ7kNvwFQPLje\u0026_nc_oc=AdnQz9vSFYwQiYmOEoNV_zBWk6clY8me1Wm4qI5jQAVmMmPkx3Szd6EtK7rJ9ilfWuw\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_Aftn-GfglDlRBog67h7yYusZ0uci87JaB4bMTdD3jxMMaA\u0026oe=699B22AD",
            //              "VideoSdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQP1atvb7jwmUbDsWgQvMc5eaJYX_bR6b01pM-I0Rsws8NF0JGVLCWMvTnS49cmQ_qccU1xlRYWTOG2hK7RvcHk9AAhSqatEdqFNusMehw.mp4?_nc_cat=103\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=mz7PVnZFpd8Q7kNvwH1nwvi\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTExMzMyMTg3NDIzMTQ4NiwiYXNzZXRfYWdlX2RheXMiOjE2MCwidmlfdXNlY2FzZV9pZCI6MTA3OTksImR1cmF0aW9uX3MiOjE2LCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfvHQgnXo_qYPmd5MNnJyTHb0Qz9uPExLrzNkX_592XGLA\u0026oe=699B2217"
            //            }
            //          ],
            //          "Cards": []
            //        },
            //        "IsActive": true,
            //        "HasUserReported": false,
            //        "PageIsDeleted": false,
            //        "PageName": "The Magic Brush",
            //        "Categories": [ "UNKNOWN" ],
            //        "ContainsDigitalCreatedMedia": false,
            //        "EndDate": 1771315200,
            //        "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //        "StartDate": 1770969600,
            //        "ContainsSensitiveContent": false,
            //        "Url": "https://www.facebook.com/ads/library?id=1673990680285512",
            //        "StartDateString": "2026-02-13T08:00:00.000Z",
            //        "EndDateString": "2026-02-17T08:00:00.000Z"
            //      },
            //      {
            //        "AdArchiveId": "1211931013961670",
            //        "CollationId": "1104694201152610",
            //        "PageId": "294643010396342",
            //        "Snapshot": {
            //          "PageId": "294643010396342",
            //          "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //          "PageName": "The Magic Brush",
            //          "PageProfilePictureUrl": "https://scontent-sjc6-1.xx.fbcdn.net/v/t39.35426-6/537396336_4053792711434136_8700917304993101717_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=107\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=zQC3qoX_DW8Q7kNvwF85ZHx\u0026_nc_oc=AdkJnzcfDZXlHjrQ1syZgd7tnu5myAun8Nx1fKQ9jSqBKc0-NaZXeTE0faz-HlJd9kM\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc6-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AftpKB_34RDTvRnglWnyXhn7rYja7GvcxnSX5lQUn36p2w\u0026oe=699B0236",
            //          "DisplayFormat": "VIDEO",
            //          "PageCategories": [ "Pet Store" ],
            //          "PageLikeCount": 7477,
            //          "IsReshared": false,
            //          "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //          "CtaType": "SHOP_NOW",
            //          "CtaText": "Shop now",
            //          "Caption": "magicbrushofficial.com",
            //          "LinkDescription": null,
            //          "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //          "Title": "\uD83D\uDEA8 BUY 1 GET 1 ENDS TODAY! \uD83D\uDEA8",
            //          "OriginalImageUrl": null,
            //          "ResizedImageUrl": null,
            //          "Images": [],
            //          "Videos": [
            //            {
            //              "VideoHdUrl": "https://video-sjc6-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQPbr-TbaFG6ipGlyt6AjtM26paf0_feOvHQSeufBKki5U5V9QqUMzFnNjCl9xzKFH-6jc4prbCjaF_pxMkFg5Zc6Q-a15xOEOG3MKuIRlJOsg.mp4?_nc_cat=104\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc6-1.xx.fbcdn.net\u0026_nc_ohc=j4yWbfh6NTUQ7kNvwGep3m1\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjE3NzE4OTA4NzY3NjE0NzMsImFzc2V0X2FnZV9kYXlzIjoxNzksInZpX3VzZWNhc2VfaWQiOjEwNzk5LCJkdXJhdGlvbl9zIjo1MSwidXJsZ2VuX3NvdXJjZSI6Ind3dyJ9\u0026ccb=17-1\u0026vs=b22c89786956ab11\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9EQjRDNTVGQ0RERUE4Q0VEODM5QThDRkNDMjA4QzdBNl9tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50LzhGNEEyRUIwNTBENTdBMjhEMTc5RUM0RjdERjM0OThCX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACaCltLkz-GlBhUCKAJDMywXQEmEOVgQYk4YGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZd6oAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfsAld6l5Yt5y4ds1UUhFjzSSV9brZDOlByrj-tQWUuMZQ\u0026oe=699B21F0",
            //              "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/537478688_2569965090037732_819948677684452553_n.jpg?_nc_cat=100\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=4fsS7z4eclwQ7kNvwFWQdGg\u0026_nc_oc=AdnXYZqsm8kI2q6fG2D0FmdZj7g_M2aRfn6eakSSxDoE_lumLQlXSFPCjldnWeguqYI\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfszrjhBe3xbT_Gzi5Kc5qs5o1Uu4qoMmV4OnZ1Jme731w\u0026oe=699B30C2",
            //              "VideoSdUrl": "https://video-sjc6-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQNIY7GpY1usjQdq9iqbxvJ8s_YlR5cgPAhURTAx6zW-36Q2-H9NVSLmgpYef-TLJ7d4XdBkmTvV4wgNnmFcamDm7X3B0Q8fPftIV3lTNQ.mp4?_nc_cat=104\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc6-1.xx.fbcdn.net\u0026_nc_ohc=pg_XB5y-xm4Q7kNvwHsg3XF\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTc3MTg5MDg3Njc2MTQ3MywiYXNzZXRfYWdlX2RheXMiOjE3OSwidmlfdXNlY2FzZV9pZCI6MTA3OTksImR1cmF0aW9uX3MiOjUxLCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfsKYFqf91iXFLAvxhJBSLEA6Hhz-rGdEFmsl1_a574oRw\u0026oe=699B148A"
            //            }
            //          ],
            //          "Cards": []
            //        },
            //        "IsActive": true,
            //        "HasUserReported": false,
            //        "PageIsDeleted": false,
            //        "PageName": "The Magic Brush",
            //        "Categories": [ "UNKNOWN" ],
            //        "ContainsDigitalCreatedMedia": false,
            //        "EndDate": 1771315200,
            //        "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //        "StartDate": 1755932400,
            //        "ContainsSensitiveContent": false,
            //        "Url": "https://www.facebook.com/ads/library?id=1211931013961670",
            //        "StartDateString": "2025-08-23T07:00:00.000Z",
            //        "EndDateString": "2026-02-17T08:00:00.000Z"
            //      },
            //      {
            //        "AdArchiveId": "4771378853182285",
            //        "CollationId": "1134436051988038",
            //        "PageId": "294643010396342",
            //        "Snapshot": {
            //          "PageId": "294643010396342",
            //          "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //          "PageName": "The Magic Brush",
            //          "PageProfilePictureUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/629733920_4306473782974582_5377999549716268893_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=106\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=3_VH8y8b8P4Q7kNvwHoZcZB\u0026_nc_oc=AdkBKwDJFmSbll76ncp_2ZX9M3l_Nax084devNhxAd4XZYfB4Z7hkvE9J2tIZ3EXty4\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfsB0WKMJfrXU1NqXwVJE3_XWK-7dZDgPEDo4XzMmxHfwA\u0026oe=699B2A79",
            //          "DisplayFormat": "VIDEO",
            //          "PageCategories": [ "Pet Store" ],
            //          "PageLikeCount": 7477,
            //          "IsReshared": false,
            //          "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //          "CtaType": "SHOP_NOW",
            //          "CtaText": "Shop now",
            //          "Caption": "magicbrushofficial.com",
            //          "LinkDescription": null,
            //          "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //          "Title": "We Can\u0027t Believe How Viral our Magic Brush Has Gone!",
            //          "OriginalImageUrl": null,
            //          "ResizedImageUrl": null,
            //          "Images": [],
            //          "Videos": [
            //            {
            //              "VideoHdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQN4qNURaFa-mJabqNmHO79E0J2bNcuCXd9j3EYDpWBttAIN1O1fryOtskBBYNiYbyX6Lp5-SJyStp0lwYFmvMBTbFgP2ny4LWhO_v8phrIA3w.mp4?_nc_cat=105\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=nQocJNzbJ6sQ7kNvwGpMlkX\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjEyMzI4MzA0NDkwNTgwMDMsImFzc2V0X2FnZV9kYXlzIjoxMSwidmlfdXNlY2FzZV9pZCI6MTAxMzksImR1cmF0aW9uX3MiOjQ5LCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026vs=e1a3f97fee3f640c\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9CRjRBRDIwQzAxODFCNkIzQ0M1Q0ExRkNCQUM4ODc4Nl9tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50LzIwNDE5Q0NENjM0NTkzMkYxMERCMzk5QkU4M0Q3QjgzX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACamg-DPltCwBBUCKAJDMywXQEjZmZmZmZoYGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZbaeAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_Afv3bDMiZpVqdGwyk9Zhhuzhfduz1tYV1DYgsZlJC41DGw\u0026oe=699B11DB",
            //              "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/628418219_905930095353371_1130105990814021226_n.jpg?_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=YcTnwXJ40NQQ7kNvwFkwLNd\u0026_nc_oc=Adn6BoZCZGGflYO-8q1P7MUck3Y7LC7TPojeuMzJaQ_pVJmlsiHlDZ_OijQ07-ppCYQ\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfuEzVaFXZesfqlI4fDbvcYJMVMOAYafQceJLur6derBDQ\u0026oe=699B2C72",
            //              "VideoSdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m412/AQOuzepP0BwhaaOYFm-Wb-WrameZ2YK_dmDqrsxkkHIqS491bNNCKeUy0KCcq4O7-WwOjar6vEL09u-AJf0RJ40QqvV8LzX01fvol7IdEg.mp4?_nc_cat=103\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=CT46unDTBigQ7kNvwF8B48r\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTIzMjgzMDQ0OTA1ODAwMywiYXNzZXRfYWdlX2RheXMiOjExLCJ2aV91c2VjYXNlX2lkIjoxMDEzOSwiZHVyYXRpb25fcyI6NDksInVybGdlbl9zb3VyY2UiOiJ3d3cifQ%3D%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AftZ6uzYI7YAXLE5gtpt9pdON2kFzWiJjoNoW-HgAeFrgQ\u0026oe=699AFCBF"
            //            }
            //          ],
            //          "Cards": []
            //        },
            //        "IsActive": true,
            //        "HasUserReported": false,
            //        "PageIsDeleted": false,
            //        "PageName": "The Magic Brush",
            //        "Categories": [ "UNKNOWN" ],
            //        "ContainsDigitalCreatedMedia": false,
            //        "EndDate": 1771315200,
            //        "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //        "StartDate": 1770364800,
            //        "ContainsSensitiveContent": false,
            //        "Url": "https://www.facebook.com/ads/library?id=4771378853182285",
            //        "StartDateString": "2026-02-06T08:00:00.000Z",
            //        "EndDateString": "2026-02-17T08:00:00.000Z"
            //      }
            //    ]
            //  }
            //}
            //""";

            //string input = """
            //                    {
            //      "SourceCountry": "US",
            //      "TargetCountry": "AU",
            //      "Keyword": "pet grooming supplies",
            //      "Product": {
            //        "ClusterId": "cl_magicbrushofficial_com_magic_brush",
            //        "ClusterLabel": "The Magic Brush",
            //        "LandingPageDomain": "magicbrushofficial.com",
            //        "LikelyProductName": "The Magic Brush",
            //        "CategoryGuess": "Pet hair remover brush/glove",
            //        "KnownFeatures": [ "soft brush cloth bristles generate static", "removes pet hair from furniture, clothing, and car interiors", "adjustable wrist strap", "reusable and easy to clean", "buy 1 get 1 free offer" ],
            //        "ClusterConfidence": "High",
            //        "AdArchiveIds": [ "1211931013961670", "1280599013387623", "1673990680285512", "4771378853182285" ]
            //      },
            //      "Ad": {
            //        "AdArchiveId": "1280599013387623",
            //        "CollationId": "1117274810507775",
            //        "PageId": "294643010396342",
            //        "Snapshot": {
            //          "PageId": "294643010396342",
            //          "PageProfileUri": "https://www.facebook.com/61558189937295/",
            //          "PageName": "The Magic Brush",
            //          "PageProfilePictureUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/545586119_793033416590173_2197844232583729508_n.jpg?stp=dst-jpg_s60x60_tt6\u0026_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=_qM5BrDVXjQQ7kNvwH_G_Cm\u0026_nc_oc=AdmqDPCE1aOGI6Z7tQKv9Q8L0WjFQK4ptfu76S_e4F9wBpsZRB1EhKCQjxl9rnt14Xo\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_Aft_5UAWTd5yBrhKwdoemK0t8rtHUs1wuzkHUv1yae3Qbg\u0026oe=699B0699",
            //          "DisplayFormat": "VIDEO",
            //          "PageCategories": [ "Pet Store" ],
            //          "PageLikeCount": 7477,
            //          "IsReshared": false,
            //          "Body": { "Text": "\uD83D\uDEA8 BUY 1 GET 1 FREE ENDS TODAY! \uD83D\uDEA8\n\n\u2705 Effortless Fur Removal: Soft brush cloth bristles generate static to lift and capture pet hair instantly.\n\u2705 Universal Fit: Ergonomic design with adjustable wrist strap ensures a snug fit for all hand sizes.\n\u2705 Reusable \u0026 Easy to Clean: Simply rinse under running water and air-dry for repeated use.\n\u2705 Versatile Use: Effectively removes hair from furniture, clothing, car interiors, and more.\n\nBuy One Get One Free Sale Ends Today! \uD83D\uDEA8" },
            //          "CtaType": "SHOP_NOW",
            //          "CtaText": "Shop now",
            //          "Caption": "magicbrushofficial.com",
            //          "LinkDescription": null,
            //          "LinkUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //          "Title": "\uD83D\uDEA8 BUY 1 GET 1 ENDS TODAY! \uD83D\uDEA8",
            //          "OriginalImageUrl": null,
            //          "ResizedImageUrl": null,
            //          "Images": [],
            //          "Videos": [
            //            {
            //              "VideoHdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQPrgttQ4KVfF3age_rdVNsVhHIdBrTk5y_VvqEq4JnnL3DDVuOSSuO2-2iy6GYi9j-SdT1n2LPzFLI2zWBf173M42BC5j0AP1_gg41ziRZK6A.mp4?_nc_cat=102\u0026_nc_sid=5e9851\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=k6fDRFmIj9cQ7kNvwHyinFq\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuNzIwLmRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHAiLCJ4cHZfYXNzZXRfaWQiOjExMTMzMjE4NzQyMzE0ODYsImFzc2V0X2FnZV9kYXlzIjoxNjAsInZpX3VzZWNhc2VfaWQiOjEwNzk5LCJkdXJhdGlvbl9zIjoxNiwidXJsZ2VuX3NvdXJjZSI6Ind3dyJ9\u0026ccb=17-1\u0026vs=57ab526d01523d13\u0026_nc_vs=HBksFQIYRWZiX2VwaGVtZXJhbC9CQjQwRDJBNzdDRTU0QUI1N0ZBOTc4QjZEMUQxMUNBM19tdF8xX3ZpZGVvX2Rhc2hpbml0Lm1wNBUAAsgBEgAVAhhAZmJfcGVybWFuZW50L0IyNDAzMzlCMDRGREZBOEU2OEI0QkJCN0VFNDRGQjhDX2F1ZGlvX2Rhc2hpbml0Lm1wNBUCAsgBEgAoABgAGwKIB3VzZV9vaWwBMRJwcm9ncmVzc2l2ZV9yZWNpcGUBMRUAACb8wqyt7qP6AxUCKAJDMywXQDDzMzMzMzMYGWRhc2hfaDI2NC1iYXNpYy1nZW4yXzcyMHARAHUAZd6oAQA\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_Afu_Jkh5u3BE9NgalZgo2BhG_4WLMbUm3_4VbCqGoend2w\u0026oe=699B2E3E",
            //              "VideoPreviewImageUrl": "https://scontent-sjc3-1.xx.fbcdn.net/v/t39.35426-6/545665919_1185840076755886_3928460495946496098_n.jpg?_nc_cat=103\u0026ccb=1-7\u0026_nc_sid=c53f8f\u0026_nc_ohc=oEJfbgSS_-8Q7kNvwGnjq79\u0026_nc_oc=Adk4a4Bl0RvVhgOQmjKmQuQ0oC7fLdosH0a-aF2S-HDHkppXtcjF-pM9HsdWwYFFlLo\u0026_nc_zt=14\u0026_nc_ht=scontent-sjc3-1.xx\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026oh=00_AfvFu_gatC3S1i4e8IZ0wXsrpSK2jNfke0QKpUNoX78XHQ\u0026oe=699B0A25",
            //              "VideoSdUrl": "https://video-sjc3-1.xx.fbcdn.net/o1/v/t2/f2/m366/AQP1atvb7jwmUbDsWgQvMc5eaJYX_bR6b01pM-I0Rsws8NF0JGVLCWMvTnS49cmQ_qccU1xlRYWTOG2hK7RvcHk9AAhSqatEdqFNusMehw.mp4?_nc_cat=103\u0026_nc_sid=8bf8fe\u0026_nc_ht=video-sjc3-1.xx.fbcdn.net\u0026_nc_ohc=mz7PVnZFpd8Q7kNvwH1nwvi\u0026efg=eyJ2ZW5jb2RlX3RhZyI6Inhwdl9wcm9ncmVzc2l2ZS5WSV9VU0VDQVNFX1BST0RVQ1RfVFlQRS4uQzMuMzYwLnN2ZV9zZCIsInhwdl9hc3NldF9pZCI6MTExMzMyMTg3NDIzMTQ4NiwiYXNzZXRfYWdlX2RheXMiOjE2MCwidmlfdXNlY2FzZV9pZCI6MTA3OTksImR1cmF0aW9uX3MiOjE2LCJ1cmxnZW5fc291cmNlIjoid3d3In0%3D\u0026ccb=17-1\u0026_nc_gid=mkde9y9FqKa0Zf-gKj2zpg\u0026_nc_zt=28\u0026oh=00_AfvHQgnXo_qYPmd5MNnJyTHb0Qz9uPExLrzNkX_592XGLA\u0026oe=699B2217"
            //            }
            //          ],
            //          "Cards": []
            //        },
            //        "IsActive": true,
            //        "HasUserReported": false,
            //        "PageIsDeleted": false,
            //        "PageName": "The Magic Brush",
            //        "Categories": [ "UNKNOWN" ],
            //        "ContainsDigitalCreatedMedia": false,
            //        "EndDate": 1771315200,
            //        "PublisherPlatform": [ "FACEBOOK", "INSTAGRAM", "AUDIENCE_NETWORK", "MESSENGER", "THREADS" ],
            //        "StartDate": 1757487600,
            //        "ContainsSensitiveContent": false,
            //        "Url": "https://www.facebook.com/ads/library?id=1280599013387623",
            //        "StartDateString": "2025-09-10T07:00:00.000Z",
            //        "EndDateString": "2026-02-17T08:00:00.000Z"
            //      }
            //    }
            //    """;

            //string input = """
            //                    {
            //      "Url": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //      "FinalUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free",
            //      "RetrievalDateTimeUtc": "2026-04-01T06:57:05Z",
            //      "Product": {
            //        "Name": "The Magic Brush\u2122 - Buy 1 Get 1 Free",
            //        "Price": 37,
            //        "Currency": "NZD",
            //        "Variants": [],
            //        "BundleOffers": [ "Buy 1 Get 1 Free \u2014 sale price NZD $37.00 vs regular price NZD $74.00", "Fur Free Home Bundle (Most Popular) \u2014 NZD $53.85 vs NZD $136.00; includes 2x The Magic Brush\u2122, 4x Pet Hair Catcher for Washing Machine, and 1x Magic Grooming Glove" ],
            //        "KeyClaims": [ "Gets rid of Cat \u0026 Dog Hair within Seconds", "Reusable \u0026 Easy to Clean", "Works on carpets, rugs, upholstery, clothing, bedding, car interiors, and pet beds", "Lifts fur without scratching or snagging", "Compact and handbag-friendly for pet-hair removal on the go", "Adjustable fit for most adult hand sizes", "Ambidextrous design for left- or right-hand use", "Reduces hair cleanup from hours to just minutes" ],
            //        "IngredientsOrMaterials": [ "Durable mesh cloth backing", "Soft brush cloth bristles", "Food-grade silicone bristles", "Reinforced stitching", "Polyester cuff" ],
            //        "Images": [ "https://magicbrushofficial.com/cdn/shop/files/MagicBrush_Logo_Colour_23727271-a98a-4d81-ac29-d3b7cdf4d759.png?v=1749229574\u0026width=280", "https://magicbrushofficial.com/cdn/shop/files/Untitled_design_1d3ad57d-0e64-4484-bb91-d6f6d9def06e.png?v=1768605293\u0026width=1200", "https://magicbrushofficial.com/cdn/shop/files/GMP_U2F2ZUdIMDE.gif?v=1768605293\u0026width=300", "https://magicbrushofficial.com/cdn/shop/files/GMP_Q29tcHJlc3NHSDAx_7_d520a39a-1b89-4141-82e0-4960d14e1246.gif?v=1768605293\u0026width=300", "https://magicbrushofficial.com/cdn/shop/files/The_Magic_Brush_6.png?v=1768605293\u0026width=600", "https://magicbrushofficial.com/cdn/shop/files/GMP_U2F2ZUdIMDE_2.gif?v=1768605293\u0026width=300", "https://magicbrushofficial.com/cdn/shop/files/GMP_Q29tcHJlc3NHSDAx_7_9_9ae9141d-73d1-4a40-a2ef-cb6782aa572d.gif?v=1768605293\u0026width=300", "https://magicbrushofficial.com/cdn/shop/files/GMP_Q29tcHJlc3NHSDAx_7_7.gif?v=1768605293\u0026width=500" ],
            //        "Videos": [ "https://cdn.shopify.com/videos/c/o/v/f05f8584753547c5964b6737bd48efa8.mp4" ],
            //        "HowItWins": {
            //          "Positioning": [ "The Magic Brush\u2122 pet grooming glove", "The ultimate choice for anyone who wants a quick, comfortable, and stylish way to banish pet hair", "National Leader" ],
            //          "Differentiators": [ "360\u00B0 silicone bristles grip and lift fur without scratching or snagging", "Bristles generate static to lift and capture fur on contact", "Adjustable Velcro wrist strap conforms to most adult hand sizes", "Ambidextrous glove design", "Compact glove lies flat for handbag, backpack, or car-console storage" ],
            //          "ProofPoints": [ "4.8 rating from 9,577 reviews", "Verified photo and video reviews are shown on-page", "Over 100,000 Happy Customers", "30-day money-back guarantee", "Refund policy says approved refunds are issued to the original payment method within 10 business days" ],
            //          "TargetCustomer": [ "Cat \u0026 dog owners", "Anyone who wants a quick, comfortable, and stylish way to banish pet hair" ],
            //          "ObjectionHandling": [ "Works on virtually any fabric surface\u2014carpets, rugs, upholstery, clothing, bedding, car interiors, and even pet beds.", "The glove comes with an adjustable Velcro wrist strap that conforms to most adult hand sizes.", "Designed to be ambidextrous\u2014simply switch the glove to whichever hand you prefer.", "Just rinse the glove under warm water to release trapped fur, then let it air-dry; it\u2019s ready again within minutes.", "You can confidently use it on delicate upholstery, silk blends, and soft clothing without fear of damage.", "Shipping policy says average shipping time is 5\u201315 days after purchase.", "Refund policy says you have 30 days after receiving your item to request a return." ],
            //          "ComparisonClaims": [ "\u201CIt\u2019s pulled up fur from my rug that a vacuum can\u2019t lift.\u201D", "\u201CThe vacuum only picks up so much.\u201D", "\u201CThe photo shows all the hair I got after the vacuuming!\u201D" ],
            //          "CallsToAction": [ "ADD TO CART" ],
            //          "OffersAndGuarantees": [ "Sale price NZD $37.00 vs regular price NZD $74.00 ($37.00 OFF)", "Buy 1 Get 1 Free", "Fur Free Home Bundle \u2014 NZD $53.85 vs NZD $136.00; includes 2x The Magic Brush\u2122, 4x Pet Hair Catcher for Washing Machine, and 1x Magic Grooming Glove", "30-Day Guarantee / 30 Days Money Back Guarantee", "FAQ guarantee text says if you\u2019re not satisfied within 30 days, they\u2019ll refund within 24 hours\u2014\u201Cno questions asked\u201D" ],
            //          "RiskReducers": [ "30-day money-back guarantee", "\u201CNot satisfied? Get a full refund within 30 days\u2014no hassle, no worries.\u201D", "24 / 7 Customer Service", "Shipping, refund, terms, and privacy policies are linked in the footer", "Shipping policy says order tracking is available via the site navigation", "Refund policy says a return shipping label is provided if the return is accepted" ],
            //          "SocialProof": [ "4.8 stars from 9,577 reviews", "Verified purchase photo and video reviews", "\u201CAbsolutely brilliant. removes fur easily. game changer\u201D \u2014 Charlotte D.", "\u201CDoes EXACTLY what the ads show!\u201D \u2014 Stina J.", "\u201CSuper duper cat hair remover! Just like the ad\u201D \u2014 Carolyn B." ],
            //          "UrgencyScarcity": [ "\uD83D\uDEA8 Buy 1 Get 1 Free Ends When Stock Runs Out! \uD83D\uDEA8", "Only 9 Magic Brushes Left", "\u201CDue to the extremely high surge in sales, we only have limited stock available per customer.\u201D" ],
            //          "ComplianceLanguage": [ "Refund policy says returns must be requested within 30 days after receiving the item.", "Returned items must be unworn or unused, with tags, in original packaging, and accompanied by proof of purchase.", "Refund policy says sale items or gift cards cannot be returned.", "EU customers have a 14 day cooling off period.", "Approved refunds are issued to the original payment method within 10 business days." ]
            //        }
            //      },
            //      "Vendor": {
            //        "BrandName": "The Magic Brush",
            //        "Domain": "magicbrushofficial.com",
            //        "PlatformDetected": "Shopify",
            //        "PolicyUrls": {
            //          "Shipping": "https://magicbrushofficial.com/policies/shipping-policy",
            //          "Returns": "https://magicbrushofficial.com/policies/refund-policy",
            //          "Privacy": "https://magicbrushofficial.com/policies/privacy-policy"
            //        },
            //        "TrustSignals": [
            //          {
            //            "Type": "rating",
            //            "Value": "4.8 rating from 9,577 reviews",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          },
            //          {
            //            "Type": "guarantee",
            //            "Value": "30-day money-back guarantee",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          },
            //          {
            //            "Type": "support",
            //            "Value": "24 / 7 Customer Service",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          },
            //          {
            //            "Type": "email",
            //            "Value": "info@magicbrushofficial.com",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          },
            //          {
            //            "Type": "shipping",
            //            "Value": "Express Shipping: From Us To Your Door In 6-10 Days",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          },
            //          {
            //            "Type": "social",
            //            "Value": "Facebook profile/reviews linked",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          },
            //          {
            //            "Type": "social",
            //            "Value": "Instagram profile linked",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          },
            //          {
            //            "Type": "social",
            //            "Value": "TikTok profile linked",
            //            "SourceUrl": "https://magicbrushofficial.com/products/the-magic-brush-buy-1-get-1-free"
            //          }
            //        ]
            //      },
            //      "Blockers": [ "Materials descriptions conflict on-page: one section says \u0027soft brush cloth bristles\u0027 while FAQ says \u0027food-grade silicone bristles\u0027.", "Customer-count claims conflict on-page: PDP says \u0027Over 100,000 Happy Customers\u0027 while footer/policy pages say \u0027Over 50,000 Happy Customers\u0027.", "Support email differs across pages: PDP/footer shows info@magicbrushofficial.com, while refund policy lists info@themagicbrush.com.", "Guarantee terms conflict: PDP promises a 30-day money-back guarantee, but refund policy says sale items cannot be returned." ]
            //    }
            //    """;

            //string input = """
            //    {"Photos":"C:\\Users\\Wen\\Downloads\\merged-image-2026-04-01T08-38-54_processed_by_imagy.png"}
            //    """;

            string input = """
                {
                    "CompetitorUsedProductName": "The Magic Brush",
                    "COGSPerUnit": "$5",
                    "TargetMarketCountry": "AU",
                    "ExtraUnitCOGSPerOrder": "$1",
                    "SalesTax": "10%",
                    "PaymentProcessingFees": "3% + $0.30",
                    "CompetitorClaims": [ "Gets rid of Cat \u0026 Dog Hair within Seconds", "Reusable \u0026 Easy to Clean", "Works on carpets, rugs, upholstery, clothing, bedding, car interiors, and pet beds", "Lifts fur without scratching or snagging", "Compact and handbag-friendly for pet-hair removal on the go", "Adjustable fit for most adult hand sizes", "Ambidextrous design for left- or right-hand use", "Reduces hair cleanup from hours to just minutes" ],
                    "IngredientsOrMaterials": [ "Durable mesh cloth backing", "Soft brush cloth bristles", "Food-grade silicone bristles", "Reinforced stitching", "Polyester cuff" ],
                    "CompetitorMarketingHowItWins": {
                      "Positioning": [ "The Magic Brush\u2122 pet grooming glove", "The ultimate choice for anyone who wants a quick, comfortable, and stylish way to banish pet hair", "National Leader" ],
                      "Differentiators": [ "360\u00B0 silicone bristles grip and lift fur without scratching or snagging", "Bristles generate static to lift and capture fur on contact", "Adjustable Velcro wrist strap conforms to most adult hand sizes", "Ambidextrous glove design", "Compact glove lies flat for handbag, backpack, or car-console storage" ],
                      "ProofPoints": [ "4.8 rating from 9,577 reviews", "Verified photo and video reviews are shown on-page", "Over 100,000 Happy Customers", "30-day money-back guarantee", "Refund policy says approved refunds are issued to the original payment method within 10 business days" ],
                      "TargetCustomer": [ "Cat \u0026 dog owners", "Anyone who wants a quick, comfortable, and stylish way to banish pet hair" ],
                      "ObjectionHandling": [ "Works on virtually any fabric surface\u2014carpets, rugs, upholstery, clothing, bedding, car interiors, and even pet beds.", "The glove comes with an adjustable Velcro wrist strap that conforms to most adult hand sizes.", "Designed to be ambidextrous\u2014simply switch the glove to whichever hand you prefer.", "Just rinse the glove under warm water to release trapped fur, then let it air-dry; it\u2019s ready again within minutes.", "You can confidently use it on delicate upholstery, silk blends, and soft clothing without fear of damage.", "Shipping policy says average shipping time is 5\u201315 days after purchase.", "Refund policy says you have 30 days after receiving your item to request a return." ],
                      "ComparisonClaims": [ "\u201CIt\u2019s pulled up fur from my rug that a vacuum can\u2019t lift.\u201D", "\u201CThe vacuum only picks up so much.\u201D", "\u201CThe photo shows all the hair I got after the vacuuming!\u201D" ],
                      "CallsToAction": [ "ADD TO CART" ],
                      "RiskReducers": [ "30-day money-back guarantee", "\u201CNot satisfied? Get a full refund within 30 days\u2014no hassle, no worries.\u201D", "24 / 7 Customer Service", "Shipping, refund, terms, and privacy policies are linked in the footer", "Shipping policy says order tracking is available via the site navigation", "Refund policy says a return shipping label is provided if the return is accepted" ],
                      "SocialProof": [ "4.8 stars from 9,577 reviews", "Verified purchase photo and video reviews", "\u201CAbsolutely brilliant. removes fur easily. game changer\u201D \u2014 Charlotte D.", "\u201CDoes EXACTLY what the ads show!\u201D \u2014 Stina J.", "\u201CSuper duper cat hair remover! Just like the ad\u201D \u2014 Carolyn B." ],
                      "UrgencyScarcity": [ "\uD83D\uDEA8 Buy 1 Get 1 Free Ends When Stock Runs Out! \uD83D\uDEA8", "Only 9 Magic Brushes Left", "\u201CDue to the extremely high surge in sales, we only have limited stock available per customer.\u201D" ],
                      "ComplianceLanguage": [ "Refund policy says returns must be requested within 30 days after receiving the item.", "Returned items must be unworn or unused, with tags, in original packaging, and accompanied by proof of purchase.", "Refund policy says sale items or gift cards cannot be returned.", "EU customers have a 14 day cooling off period.", "Approved refunds are issued to the original payment method within 10 business days." ]
                    }
                }
                """;

            string result = await workflow.RunAsync(conversationID.ToString(), input, cancellationToken);
            System.Console.WriteLine(result);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
