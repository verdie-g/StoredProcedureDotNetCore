CREATE TABLE public.table_1 (
    id                   BIGSERIAL     PRIMARY KEY,
    name                 VARCHAR(255)  NULL,
    date                 TIMESTAMPTZ   NOT NULL,
    active               BOOLEAN       NOT NULL,
    name_with_underscore INT           NOT NULL DEFAULT 0
);
