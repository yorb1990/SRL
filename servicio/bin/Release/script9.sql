	BEGIN TRY
		DECLARE 
			@id_per numeric(18,0),
			@carpeta varchar(max),
			@nombre varchar(max),
			@paterno varchar(max),
			@materno varchar(max),
			@nombretemp varchar(255),
			@origen varchar(50),
			@NumberRecords int,
            @RowCount int,
            @NumberRecords2 int,
            @RowCount2 int
        declare @personas table (
		        rowId int IDENTITY(1, 1), 
		        id_per numeric(18,0),
		        nombre varchar(max),
		        paterno varchar(max),
		        materno varchar(max)
		    )
        declare @coincidencias table(
		        rowId int IDENTITY(1, 1), 
		        nombre varchar(max),
		        carpeta varchar(max)
		    )
        declare @datatable table(
                id_per int,
                nombre varchar(max),
                carpeta varchar(max),
                origen varchar(50)
            )
            

		INSERT INTO @personas (id_per, nombre, paterno,materno)
		select id_per,rtrim(nombre) nombre,rtrim(a_paterno) paterno,rtrim(a_materno) materno from dbo.personas
		SET @NumberRecords = @@ROWCOUNT
		SET @RowCount = 1
		set @origen='XALAPA'
		WHILE @RowCount <= @NumberRecords
		BEGIN
			SELECT @id_per=id_per, @nombre = nombre, @paterno = paterno, @materno = materno
			FROM @personas
			WHERE RowID = @RowCount
			    if(isnull(@nombre,'1')<>'1')
			    begin
				    set @nombretemp=CONCAT(replace(rtrim(@nombre),' ',' AND '),'')
			    end
			    if(isnull(@paterno,'1')<>'1') and (replace(rtrim(@paterno),' ','')<>'')
			    begin
				    set @nombretemp=CONCAT(@nombretemp,' AND ',replace(rtrim(@paterno),' ',''))
    			end
			    if(isnull(@materno,'1')<>'1')
			    begin
				    set @nombretemp=CONCAT(@nombretemp,' AND ',replace(rtrim(@materno),' ',''))
			    end
			    set @nombretemp=rtrim(@nombretemp)
			    if(SUBSTRING(rtrim(@nombretemp) ,LEN(@nombretemp)-3 , 4)=' AND')
			    begin
				    set @nombretemp=SUBSTRING(@nombretemp,0,LEN(@nombretemp)-3)
			    end
			    if(SUBSTRING(rtrim(@nombretemp) ,0 , 4)='AND ')
			    begin
				    set @nombretemp=SUBSTRING(@nombretemp,4,LEN(@nombretemp))
			    end
			    INSERT INTO @coincidencias (nombre, carpeta)
			    SELECT distinct [nombre],[carpeta]
			        FROM  dbo.internaresumen
			        where origen=@origen
			        and contains(NOMBRE,@nombretemp)
			    SET @NumberRecords2 = @@ROWCOUNT
			    SET @RowCount2 = 1
			    --print concat(@NumberRecords2,' posibles para persona id:',@id_per)
			    WHILE @RowCount2 <= @NumberRecords2
			    BEGIN
				    SELECT @carpeta = carpeta,
                    @nombretemp=nombre
				        FROM @coincidencias
				        WHERE rowId = @RowCount2
				    insert into @datatable(id_per,nombre,carpeta,origen) values(@id_per,@nombretemp,@carpeta,@origen)
				    SET @RowCount2 = @RowCount2 + 1
			    END
			    --truncate table #coincidencias			
			    SET @RowCount = @RowCount + 1
		    END
            select * from @datatable
	    END TRY
	BEGIN CATCH
		SELECT ERROR_NUMBER() AS ErrorNumber
		 ,ERROR_SEVERITY() AS ErrorSeverity
		 ,ERROR_STATE() AS ErrorState
		 ,ERROR_PROCEDURE() AS ErrorProcedure
		 ,ERROR_LINE() AS ErrorLine
		 ,ERROR_MESSAGE() AS ErrorMessage;
	END CATCH