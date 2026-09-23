CREATE OR REPLACE FUNCTION fn_sequence_get_next(
    p_entity_name text,
    p_entity_id   uuid
)
    RETURNS bigint
    LANGUAGE sql
AS $$
WITH up AS (
    INSERT INTO sequences (id, entity, entity_id, counter, created_at, updated_at)
        VALUES (
           uuid_generate_v4(),
           p_entity_name,
           p_entity_id::text,
           1,
           now(),
           now()
       )
        ON CONFLICT (entity, entity_id)
            DO UPDATE SET
                counter    = sequences.counter + 1,
                updated_at = now()
        RETURNING counter
)
SELECT counter FROM up;
$$;

