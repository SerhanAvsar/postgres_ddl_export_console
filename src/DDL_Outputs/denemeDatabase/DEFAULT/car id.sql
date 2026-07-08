--

ALTER TABLE ONLY public.car ALTER COLUMN id SET DEFAULT nextval('public.car_id_seq'::regclass);


--
