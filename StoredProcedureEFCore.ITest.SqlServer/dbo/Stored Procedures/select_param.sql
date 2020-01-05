CREATE PROCEDURE [dbo].[select_param]
    @n INT
AS
BEGIN
    SELECT @n;
END