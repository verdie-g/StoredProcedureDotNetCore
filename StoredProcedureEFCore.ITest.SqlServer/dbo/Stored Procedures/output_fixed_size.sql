CREATE PROCEDURE [dbo].[output_fixed_size]
    @fixed_size VARCHAR(255) OUTPUT
AS
BEGIN
    SET @fixed_size = 'Jambon Beurre'
END