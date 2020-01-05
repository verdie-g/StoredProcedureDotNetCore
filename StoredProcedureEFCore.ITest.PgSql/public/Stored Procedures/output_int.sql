CREATE OR REPLACE FUNCTION output_int(INOUT int_to_return INT) AS $$
BEGIN
    int_to_return := int_to_return + 5;
END
$$ LANGUAGE plpgsql;
