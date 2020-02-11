CREATE PROCEDURE [dbo].[output_nullable_with_non_null_value]
  @nullable INT = NULL OUTPUT
AS
BEGIN
    SET @nullable = 12
END