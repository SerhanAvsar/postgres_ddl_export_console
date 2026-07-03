-- DDL Script for Table public.person
CREATE TABLE public.person (
    id bigint DEFAULT nextval('person_id_seq'::regclass) NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    gender character varying(5) NOT NULL,
    date_of_birth date NOT NULL
);

