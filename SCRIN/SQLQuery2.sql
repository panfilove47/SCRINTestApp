CREATE TABLE [dbo].[deliverytype] (
    [deliverytypeid] INT            NOT NULL,
    [type]           VARCHAR (4000) NOT NULL,
    PRIMARY KEY CLUSTERED ([deliverytypeid] ASC)
);

CREATE TABLE [dbo].[producttype] (
    [producttypeid] INT            NOT NULL,
    [type]          VARCHAR (4000) NOT NULL,
    PRIMARY KEY CLUSTERED ([producttypeid] ASC)
);

CREATE TABLE [dbo].[product] (
    [productid]                 INT             IDENTITY (1, 1) NOT NULL,
    [productname]               NVARCHAR (4000) NULL,
    [descriprion]               NVARCHAR (4000) NULL,
    [price]                     DECIMAL (18, 2) NULL,
    [producttype_producttypeid] INT             NULL,
    [image]                     NVARCHAR (4000) NULL,
    PRIMARY KEY CLUSTERED ([productid] ASC),
    FOREIGN KEY ([producttype_producttypeid]) REFERENCES [dbo].[producttype] ([producttypeid]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[User] (
    [userid]      INT             IDENTITY (1, 1) NOT NULL,
    [mail]        NVARCHAR (4000) NULL,
    [username]    NVARCHAR (4000) NULL,
    [password]    NVARCHAR (4000) NULL,
    [balance]     FLOAT (53)      NULL,
    [phonenumber] NVARCHAR (4000) NULL,
    PRIMARY KEY CLUSTERED ([userid] ASC)
);

CREATE TABLE [dbo].[Order] (
    [orderid]                     INT             NOT NULL,
    [user_userid]                 INT             NULL,
    [deliverytype_deliverytypeid] INT             NULL,
    [Comment]                     NVARCHAR (4000) NULL,
    [adress]                      NVARCHAR (4000) NULL,
    [orderdate]                   DATE            NULL,
    [value]                       DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([orderid] ASC),
    FOREIGN KEY ([deliverytype_deliverytypeid]) REFERENCES [dbo].[deliverytype] ([deliverytypeid]) ON DELETE CASCADE,
    FOREIGN KEY ([user_userid]) REFERENCES [dbo].[User] ([userid]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[orderlist] (
    [order_orderid]     INT NOT NULL,
    [product_productid] INT NOT NULL,
    [count]             INT NOT NULL,
    PRIMARY KEY CLUSTERED ([product_productid] ASC, [order_orderid] ASC),
    FOREIGN KEY ([product_productid]) REFERENCES [dbo].[product] ([productid]) ON DELETE CASCADE,
    FOREIGN KEY ([product_productid]) REFERENCES [dbo].[product] ([productid]),
    FOREIGN KEY ([order_orderid]) REFERENCES [dbo].[Order] ([orderid]) ON DELETE CASCADE,
    FOREIGN KEY ([order_orderid]) REFERENCES [dbo].[Order] ([orderid])
);

ALTER TABLE orderlist ADD FOREIGN KEY (order_orderid) REFERENCES [Order] (orderid) ON DELETE NO ACTION;
ALTER TABLE orderlist ADD FOREIGN KEY (product_productid) REFERENCES product (productid) ON DELETE NO ACTION;