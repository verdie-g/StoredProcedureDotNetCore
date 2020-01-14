CREATE OR REPLACE FUNCTION delayed_list_all(delay_in_seconds_before_result_set INT = 0, delay_in_seconds_after_result_set INT = 0)
    RETURNS SETOF table_1
AS $$
BEGIN
    PERFORM pg_sleep(delay_in_seconds_before_result_set);
    RETURN QUERY SELECT * FROM table_1;
    PERFORM pg_sleep(delay_in_seconds_after_result_set);
    RETURN;
END
$$ LANGUAGE plpgsql;
