-- =============================================
-- Author:		Grégoire Verdier
-- Create date: 29/03/2017
-- Description:
-- =============================================
CREATE PROCEDURE [dbo].[ListAll]
	@limit bigint = 9223372036854775807,
	@delay_in_seconds_before_resultset int = 0,
	@delay_in_seconds_after_resultset int = 0,
  @limitOut bigint = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @delay DATETIME;

	IF ISNULL(@delay_in_seconds_before_resultset, 0) > 0
	BEGIN
		SET @delay = DATEADD(SECOND, @delay_in_seconds_before_resultset, CONVERT(DATETIME, 0));
		WAITFOR DELAY @delay;
	END
	
	SET @limitOut = @limit;

	SELECT TOP(@limit) *, 5 AS extra_column FROM Table_1;

	IF ISNULL(@delay_in_seconds_after_resultset, 0) > 0
	BEGIN
		SET @delay = DATEADD(SECOND, @delay_in_seconds_after_resultset, CONVERT(DATETIME, 0));
		WAITFOR DELAY @delay;
	END
END