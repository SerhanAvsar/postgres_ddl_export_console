--

CREATE TABLE public.car (
    id bigint NOT NULL,
    model character varying(50) NOT NULL,
    version_ character varying(50) NOT NULL,
    maxdistance character varying(5) NOT NULL,
    introduce_date date NOT NULL
);


ALTER TABLE public.car OWNER TO postgres;

--
