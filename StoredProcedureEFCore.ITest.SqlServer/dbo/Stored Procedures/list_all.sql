CREATE PROCEDURE [dbo].[list_all]
    @lim BIGINT = 9223372036854775807
AS
BEGIN
    SELECT TOP(@lim) *, 5 AS extra_column FROM table_1;
END