-- Seed boarding/dropping points for all existing buses
DO $$
DECLARE
  v_bus       RECORD;
  v_boarding  TEXT;
  v_dropping  TEXT;
  v_idx       INT;

  -- Stops per city
  chennai_stops    TEXT[] := ARRAY['CMBT Koyambedu','Guindy Bus Stand','Chennai Central','Tambaram Bus Stand','Perambur Bus Stand','Vadapalani','Chrompet'];
  coimbatore_stops TEXT[] := ARRAY['Gandhipuram Bus Stand','UKKADAM Bus Stand','Townhall Bus Stand','Singanallur','Peelamedu','Podanur Junction','RS Puram'];
  madurai_stops    TEXT[] := ARRAY['Mattuthavani Bus Stand','Periyar Bus Stand','Anna Bus Stand','Arappalayam','Goripalayam'];
  pondy_stops      TEXT[] := ARRAY['New Bus Stand Pondicherry','Lawspet','Murungapakkam','Pondicherry Central','Reddiarpalayam'];
  salem_stops      TEXT[] := ARRAY['New Bus Stand Salem','Suramangalam','Salem Junction','Five Roads','Attur Road'];
  trichy_stops     TEXT[] := ARRAY['Central Bus Stand Trichy','Chatram Bus Stand','Srirangam','Ariyamangalam','Tiruverumbur'];
  tirunelveli_stops TEXT[] := ARRAY['Central Bus Stand Tirunelveli','Palayamkottai','Tirunelveli Junction','Melapalayam','Vannarpettai'];
  bangalore_stops  TEXT[] := ARRAY['Majestic Bus Stand','Shivajinagar','Kempegowda Bus Stand','Silk Board','Electronic City'];
  vellore_stops    TEXT[] := ARRAY['Katpadi Junction','VCTPL Bus Stand Vellore','Vellore Central','Sathuvachari','Bagayam'];

  updated INT := 0;
BEGIN
  FOR v_bus IN
    SELECT b."Id", r."Source", r."Destination"
    FROM "Buses" b
    JOIN "Routes" r ON b."RouteId" = r."Id"
    WHERE b."BoardingPoint" IS NULL
  LOOP
    -- Pick boarding stop based on source city
    v_boarding := CASE v_bus."Source"
      WHEN 'Chennai'     THEN chennai_stops[   (abs(hashtext(v_bus."Id"::text)::bigint) % array_length(chennai_stops,1)) + 1]
      WHEN 'Coimbatore'  THEN coimbatore_stops[(abs(hashtext(v_bus."Id"::text)::bigint) % array_length(coimbatore_stops,1)) + 1]
      WHEN 'Madurai'     THEN madurai_stops[   (abs(hashtext(v_bus."Id"::text)::bigint) % array_length(madurai_stops,1)) + 1]
      WHEN 'Pondicherry' THEN pondy_stops[     (abs(hashtext(v_bus."Id"::text)::bigint) % array_length(pondy_stops,1)) + 1]
      WHEN 'Salem'       THEN salem_stops[     (abs(hashtext(v_bus."Id"::text)::bigint) % array_length(salem_stops,1)) + 1]
      WHEN 'Trichy'      THEN trichy_stops[    (abs(hashtext(v_bus."Id"::text)::bigint) % array_length(trichy_stops,1)) + 1]
      WHEN 'Tirunelveli' THEN tirunelveli_stops[(abs(hashtext(v_bus."Id"::text)::bigint) % array_length(tirunelveli_stops,1)) + 1]
      WHEN 'Bangalore'   THEN bangalore_stops[ (abs(hashtext(v_bus."Id"::text)::bigint) % array_length(bangalore_stops,1)) + 1]
      WHEN 'Vellore'     THEN vellore_stops[   (abs(hashtext(v_bus."Id"::text)::bigint) % array_length(vellore_stops,1)) + 1]
      ELSE v_bus."Source" || ' Bus Stand'
    END;

    -- Pick dropping stop based on destination city (use different hash seed)
    v_dropping := CASE v_bus."Destination"
      WHEN 'Chennai'     THEN chennai_stops[   (abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(chennai_stops,1)) + 1]
      WHEN 'Coimbatore'  THEN coimbatore_stops[(abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(coimbatore_stops,1)) + 1]
      WHEN 'Madurai'     THEN madurai_stops[   (abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(madurai_stops,1)) + 1]
      WHEN 'Pondicherry' THEN pondy_stops[     (abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(pondy_stops,1)) + 1]
      WHEN 'Salem'       THEN salem_stops[     (abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(salem_stops,1)) + 1]
      WHEN 'Trichy'      THEN trichy_stops[    (abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(trichy_stops,1)) + 1]
      WHEN 'Tirunelveli' THEN tirunelveli_stops[(abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(tirunelveli_stops,1)) + 1]
      WHEN 'Bangalore'   THEN bangalore_stops[ (abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(bangalore_stops,1)) + 1]
      WHEN 'Vellore'     THEN vellore_stops[   (abs(hashtext(v_bus."Id"::text || 'd')::bigint) % array_length(vellore_stops,1)) + 1]
      ELSE v_bus."Destination" || ' Bus Stand'
    END;

    UPDATE "Buses"
    SET "BoardingPoint" = v_boarding,
        "DroppingPoint" = v_dropping
    WHERE "Id" = v_bus."Id";

    updated := updated + 1;
  END LOOP;

  RAISE NOTICE 'Updated % buses with boarding/dropping points', updated;
END $$;
