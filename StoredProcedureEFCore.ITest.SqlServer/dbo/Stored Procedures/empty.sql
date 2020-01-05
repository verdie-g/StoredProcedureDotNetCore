CREATE PROCEDURE [dbo].[empty]
AS
BEGIN
  SELECT * FROM table_1 WHERE id < 0;
END