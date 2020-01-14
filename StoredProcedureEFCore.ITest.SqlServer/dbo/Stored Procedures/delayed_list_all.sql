CREATE PROCEDURE [dbo].[delayed_list_all]
    @delay_in_seconds_before_result_set INT = 0,
    @delay_in_seconds_after_result_set INT = 0
AS
BEGIN
    DECLARE @delay DATETIME;

    IF ISNULL(@delay_in_seconds_before_result_set, 0) > 0
        BEGIN
            SET @delay = DATEADD(SECOND, @delay_in_seconds_before_result_set, CONVERT(DATETIME, 0));
            WAITFOR DELAY @delay;
        END

    SELECT * FROM table_1;

    IF ISNULL(@delay_in_seconds_after_result_set, 0) > 0
        BEGIN
            SET @delay = DATEADD(SECOND, @delay_in_seconds_after_result_set, CONVERT(DATETIME, 0));
            WAITFOR DELAY @delay;
        END
END
