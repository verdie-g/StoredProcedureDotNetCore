CREATE PROCEDURE [dbo].[list_not_all]
    @lim bigint = 9223372036854775807
AS
BEGIN
    SELECT TOP(@lim) id, name, 5 AS extra_column FROM table_1;
END