-- Bus Booking System: Bulk Seed (buses + seats)
DO $$
DECLARE
  v_admin_id  TEXT;
  v_route     RECORD;
  v_bus_id    UUID;
  v_seat_code TEXT;
  v_row_num   INT;
  v_col_idx   INT;
  cols        TEXT[] := ARRAY['A','B','C','D'];
  v_dep       INTERVAL;
  v_arr       INTERVAL;
  v_date      DATE;
  v_price     NUMERIC;
  v_type      TEXT;
  v_name      TEXT;
  v_seats     INT;
  v_reg       TEXT;
  v_day       INT;
  v_slot      INT;
  v_dur_hrs   INT;
  v_idx       BIGINT;

  dep_slots   INTERVAL[] := ARRAY[
    '05:30'::interval, '06:00'::interval, '07:00'::interval,
    '08:30'::interval, '10:00'::interval, '12:00'::interval,
    '14:00'::interval, '16:00'::interval, '18:00'::interval,
    '20:00'::interval, '21:30'::interval, '22:30'::interval,
    '23:00'::interval, '23:30'::interval
  ];
  bus_types   TEXT[] := ARRAY[
    'AC Seater','Non-AC Seater','Volvo AC','AC Sleeper','Semi-Sleeper'
  ];
  sfx         TEXT[] := ARRAY[
    'Express','Deluxe','Royal','Ultra','Super','Premium','Star','VIP','Fast','Comfort'
  ];

  v_bus_count  INT := 0;
  v_seat_count INT := 0;
BEGIN
  SELECT "Id" INTO v_admin_id
  FROM "AspNetUsers"
  WHERE "NormalizedEmail" = 'ADMIN@BUSBOOKING.COM'
  LIMIT 1;

  FOR v_route IN SELECT "Id", "Source", "Destination" FROM "Routes" LOOP
    v_dur_hrs := CASE
      WHEN v_route."Source"='Chennai'     AND v_route."Destination"='Coimbatore'  THEN 7
      WHEN v_route."Source"='Coimbatore'  AND v_route."Destination"='Chennai'     THEN 7
      WHEN v_route."Source"='Chennai'     AND v_route."Destination"='Madurai'     THEN 8
      WHEN v_route."Source"='Madurai'     AND v_route."Destination"='Chennai'     THEN 8
      WHEN v_route."Source"='Chennai'     AND v_route."Destination"='Pondicherry' THEN 3
      WHEN v_route."Source"='Pondicherry' AND v_route."Destination"='Chennai'     THEN 3
      WHEN v_route."Source"='Chennai'     AND v_route."Destination"='Salem'       THEN 5
      WHEN v_route."Source"='Salem'       AND v_route."Destination"='Chennai'     THEN 5
      WHEN v_route."Source"='Chennai'     AND v_route."Destination"='Trichy'      THEN 6
      WHEN v_route."Source"='Trichy'      AND v_route."Destination"='Chennai'     THEN 6
      WHEN v_route."Source"='Coimbatore'  AND v_route."Destination"='Madurai'     THEN 4
      WHEN v_route."Source"='Madurai'     AND v_route."Destination"='Coimbatore'  THEN 4
      WHEN v_route."Source"='Chennai'     AND v_route."Destination"='Tirunelveli' THEN 9
      WHEN v_route."Source"='Tirunelveli' AND v_route."Destination"='Chennai'     THEN 9
      WHEN v_route."Source"='Coimbatore'  AND v_route."Destination"='Bangalore'   THEN 5
      WHEN v_route."Source"='Bangalore'   AND v_route."Destination"='Coimbatore'  THEN 5
      WHEN v_route."Source"='Chennai'     AND v_route."Destination"='Vellore'     THEN 2
      WHEN v_route."Source"='Madurai'     AND v_route."Destination"='Trichy'      THEN 3
      ELSE 6
    END;

    FOR v_day IN 0..6 LOOP
      v_date := CURRENT_DATE + v_day;

      FOR v_slot IN 1..3 LOOP
        -- safe positive index for dep_slots (1-14)
        v_idx  := (abs(hashtext(v_route."Source" || v_route."Destination" || v_day::text || v_slot::text)::bigint) % 14) + 1;
        v_dep  := dep_slots[v_idx];
        v_arr  := v_dep + (v_dur_hrs || ' hours')::interval;

        -- bus type (1-5)
        v_idx  := (abs(hashtext(v_route."Destination" || v_day::text || v_slot::text)::bigint) % 5) + 1;
        v_type := bus_types[v_idx];

        v_seats := CASE v_type WHEN 'AC Sleeper' THEN 36 WHEN 'Non-AC Seater' THEN 52 ELSE 40 END;
        v_price := CASE v_type
          WHEN 'Volvo AC'      THEN 600 + (abs(hashtext(v_route."Source"||v_day::text||v_slot::text)::bigint) % 250)
          WHEN 'AC Sleeper'    THEN 800 + (abs(hashtext(v_route."Destination"||v_day::text||v_slot::text)::bigint) % 400)
          WHEN 'AC Seater'     THEN 400 + (abs(hashtext(v_route."Source"||v_slot::text)::bigint) % 200)
          WHEN 'Semi-Sleeper'  THEN 480 + (abs(hashtext(v_route."Source"||v_day::text)::bigint) % 170)
          ELSE                      200 + (abs(hashtext(v_route."Destination"||v_slot::text)::bigint) % 120)
        END;

        -- company name suffix (1-10)
        v_idx  := (abs(hashtext(v_route."Source"||v_day::text||v_slot::text)::bigint) % 10) + 1;
        v_name := v_route."Source" || ' ' || sfx[v_idx] || ' Travels';

        -- registration number TN + 2digit + 2letter + 4digit
        v_reg  := 'TN' ||
          lpad(((abs(hashtext(v_route."Source"||v_route."Destination"||v_day::text||v_slot::text)::bigint) % 76)+1)::text,2,'0') ||
          chr((65 + (abs(hashtext(v_route."Source"||v_slot::text)::bigint) % 26))::int) ||
          chr((65 + (abs(hashtext(v_route."Destination"||v_slot::text||v_day::text)::bigint) % 26))::int) ||
          lpad(((abs(hashtext(v_route."Source"||v_route."Destination"||v_day::text||v_slot::text||'x')::bigint) % 9000)+1000)::text,4,'0');

        -- ensure uniqueness
        IF EXISTS (SELECT 1 FROM "Buses" WHERE "RegistrationNumber" = v_reg) THEN
          v_reg := v_reg || chr(65 + v_day) || v_slot::text;
        END IF;

        v_bus_id := gen_random_uuid();

        INSERT INTO "Buses"(
          "Id","RegistrationNumber","BusName","BusType",
          "Timing","ArrivalTime",
          "TotalSeats","BasePrice","Price",
          "RouteId","OperatorId","BusOperatorId",
          "TravelDate","IsActive","CreatedAt","Status",
          "SeatLayout","CancellationReason"
        ) VALUES (
          v_bus_id, v_reg, v_name, v_type,
          v_dep, v_arr,
          v_seats, v_price, v_price,
          v_route."Id", v_admin_id, NULL,
          (v_date::timestamp AT TIME ZONE 'UTC'), TRUE, NOW(), 0,
          NULL, NULL
        );
        v_bus_count := v_bus_count + 1;

        -- insert seats (2+2 layout, rows 1..N/4, cols A-D)
        FOR v_row_num IN 1..(v_seats/4) LOOP
          FOR v_col_idx IN 1..4 LOOP
            v_seat_code := v_row_num::text || cols[v_col_idx];
            INSERT INTO "Seats"(
              "Id","BusId","SeatCode","IsAvailable","Status",
              "ScheduleId","LockedByUserId","LockedUntil",
              "BookedByUserId","LockedBySessionId"
            ) VALUES (
              gen_random_uuid(), v_bus_id, v_seat_code, TRUE, 0,
              NULL, NULL, NULL, NULL, NULL
            );
            v_seat_count := v_seat_count + 1;
          END LOOP;
        END LOOP;

      END LOOP; -- slot
    END LOOP; -- day
  END LOOP; -- route

  RAISE NOTICE 'Done: % buses, % seats', v_bus_count, v_seat_count;
END $$;
