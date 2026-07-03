-- DDL Script for Table public.car
CREATE TABLE public.car (
    id bigint DEFAULT nextval('car_id_seq'::regclass) NOT NULL,
    model character varying(50) NOT NULL,
    version_ character varying(50) NOT NULL,
    maxdistance character varying(5) NOT NULL,
    introduce_date date NOT NULL
);

