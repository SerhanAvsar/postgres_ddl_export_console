--

ALTER TABLE ONLY public.person ALTER COLUMN id SET DEFAULT nextval('public.person_id_seq'::regclass);


--
