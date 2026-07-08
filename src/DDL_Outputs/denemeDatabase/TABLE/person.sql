--

CREATE TABLE public.person (
    id bigint NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    gender character varying(5) NOT NULL,
    date_of_birth date NOT NULL
);


ALTER TABLE public.person OWNER TO postgres;

--
