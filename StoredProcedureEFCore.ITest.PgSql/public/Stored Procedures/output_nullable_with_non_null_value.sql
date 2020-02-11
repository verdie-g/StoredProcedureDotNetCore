CREATE OR REPLACE FUNCTION output_nullable_with_non_null_value(OUT nullable INT) AS $$
BEGIN
    nullable = 12;
END
$$ LANGUAGE plpgsql;
