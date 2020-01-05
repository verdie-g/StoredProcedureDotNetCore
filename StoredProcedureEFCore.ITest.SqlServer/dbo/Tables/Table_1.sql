CREATE TABLE [dbo].[Table_1] (
    [id]                   BIGINT        IDENTITY (1, 1) NOT NULL,
    [name]                 VARCHAR (255) NULL,
    [date]                 DATETIME      NOT NULL,
    [active]               BIT           NOT NULL,
    [name_with_underscore] INT           CONSTRAINT [DF_Table_1_name_with_underscore] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Table_1] PRIMARY KEY CLUSTERED ([id] ASC)
);

