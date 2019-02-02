CREATE TABLE [Aukcija]
( 
	[AukcijaID]          int  IDENTITY  NOT NULL ,
	[Proizvod]           varchar(20)  NULL ,
	[Trajanje]           int  NULL ,
	[PocetnaCena]        decimal(10,2)  NULL ,
	[VremeKreiranja]     datetime  NULL ,
	[VremeOtvaranja]     datetime  NULL ,
	[VremeZatvaranja]    datetime  NULL ,
	[Status]             varchar(20)  NULL ,
	[TrenutnaCena]       decimal(10,2)  NULL ,
	[Slika]              image  NULL ,
	[KorisnikID]         int  NULL ,
	[BidID]              int  NULL 
)
go

ALTER TABLE [Aukcija]
	ADD CONSTRAINT [XPKAukcija] PRIMARY KEY  NONCLUSTERED ([AukcijaID] ASC)
go

CREATE TABLE [Bid]
( 
	[BidID]              int  IDENTITY  NOT NULL ,
	[PonCena]            decimal(10,2)  NULL ,
	[AukcijaID]          int  NULL ,
	[KorisnikID]         int  NULL ,
	[VremeSlanja]        datetime  NULL ,
	[BrojTokena]         int  NULL 
)
go

ALTER TABLE [Bid]
	ADD CONSTRAINT [XPKBid] PRIMARY KEY  NONCLUSTERED ([BidID] ASC)
go

CREATE TABLE [Korisnik]
( 
	[KorisnikID]         int  IDENTITY  NOT NULL ,
	[Ime]                varchar(20)  NULL ,
	[Prezime]            varchar(20)  NULL ,
	[Lozinka]            varchar(60)  NULL ,
	[Email]              varchar(50)  NULL ,
	[BrojTokena]         int  NULL ,
	[Admin]              bit  NULL 
)
go

ALTER TABLE [Korisnik]
	ADD CONSTRAINT [XPKKorisnik] PRIMARY KEY  NONCLUSTERED ([KorisnikID] ASC)
go

CREATE TABLE [Narudzbina]
( 
	[NarudzbinaID]       int  IDENTITY  NOT NULL ,
	[CenaTokena]         decimal(10,2)  NULL ,
	[Status]             varchar(20)  NULL ,
	[BrojTokena]         int  NULL ,
	[DatumPravljenja]    datetime  NULL ,
	[KorisnikID]         int  NULL 
)
go

ALTER TABLE [Narudzbina]
	ADD CONSTRAINT [XPKNarudzbina] PRIMARY KEY  NONCLUSTERED ([NarudzbinaID] ASC)
go


CREATE TRIGGER tD_Aukcija ON Aukcija FOR DELETE AS
/* erwin Builtin Trigger */
/* DELETE trigger on Aukcija */
BEGIN
  DECLARE  @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)
    /* erwin Builtin Trigger */
    /* Aukcija  Bid on parent delete no action */
    /* ERWIN_RELATION:CHECKSUM="00032ae5", PARENT_OWNER="", PARENT_TABLE="Aukcija"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_3", FK_COLUMNS="AukcijaID" */
    IF EXISTS (
      SELECT * FROM deleted,Bid
      WHERE
        /*  %JoinFKPK(Bid,deleted," = "," AND") */
        Bid.AukcijaID = deleted.AukcijaID
    )
    BEGIN
      SELECT @errno  = 30001,
             @errmsg = 'Cannot delete Aukcija because Bid exists.'
      GOTO error
    END

    /* erwin Builtin Trigger */
    /* Bid  Aukcija on child delete no action */
    /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Bid"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_6", FK_COLUMNS="BidID" */
    IF EXISTS (SELECT * FROM deleted,Bid
      WHERE
        /* %JoinFKPK(deleted,Bid," = "," AND") */
        deleted.BidID = Bid.BidID AND
        NOT EXISTS (
          SELECT * FROM Aukcija
          WHERE
            /* %JoinFKPK(Aukcija,Bid," = "," AND") */
            Aukcija.BidID = Bid.BidID
        )
    )
    BEGIN
      SELECT @errno  = 30010,
             @errmsg = 'Cannot delete last Aukcija because Bid exists.'
      GOTO error
    END

    /* erwin Builtin Trigger */
    /* Korisnik  Aukcija on child delete no action */
    /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_2", FK_COLUMNS="KorisnikID" */
    IF EXISTS (SELECT * FROM deleted,Korisnik
      WHERE
        /* %JoinFKPK(deleted,Korisnik," = "," AND") */
        deleted.KorisnikID = Korisnik.KorisnikID AND
        NOT EXISTS (
          SELECT * FROM Aukcija
          WHERE
            /* %JoinFKPK(Aukcija,Korisnik," = "," AND") */
            Aukcija.KorisnikID = Korisnik.KorisnikID
        )
    )
    BEGIN
      SELECT @errno  = 30010,
             @errmsg = 'Cannot delete last Aukcija because Korisnik exists.'
      GOTO error
    END


    /* erwin Builtin Trigger */
    RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go


CREATE TRIGGER tU_Aukcija ON Aukcija FOR UPDATE AS
/* erwin Builtin Trigger */
/* UPDATE trigger on Aukcija */
BEGIN
  DECLARE  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insAukcijaID int,
           @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)

  SELECT @numrows = @@rowcount
  /* erwin Builtin Trigger */
  /* Aukcija  Bid on parent update no action */
  /* ERWIN_RELATION:CHECKSUM="0003cb94", PARENT_OWNER="", PARENT_TABLE="Aukcija"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_3", FK_COLUMNS="AukcijaID" */
  IF
    /* %ParentPK(" OR",UPDATE) */
    UPDATE(AukcijaID)
  BEGIN
    IF EXISTS (
      SELECT * FROM deleted,Bid
      WHERE
        /*  %JoinFKPK(Bid,deleted," = "," AND") */
        Bid.AukcijaID = deleted.AukcijaID
    )
    BEGIN
      SELECT @errno  = 30005,
             @errmsg = 'Cannot update Aukcija because Bid exists.'
      GOTO error
    END
  END

  /* erwin Builtin Trigger */
  /* Bid  Aukcija on child update no action */
  /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Bid"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_6", FK_COLUMNS="BidID" */
  IF
    /* %ChildFK(" OR",UPDATE) */
    UPDATE(BidID)
  BEGIN
    SELECT @nullcnt = 0
    SELECT @validcnt = count(*)
      FROM inserted,Bid
        WHERE
          /* %JoinFKPK(inserted,Bid) */
          inserted.BidID = Bid.BidID
    /* %NotnullFK(inserted," IS NULL","select @nullcnt = count(*) from inserted where"," AND") */
    select @nullcnt = count(*) from inserted where
      inserted.BidID IS NULL
    IF @validcnt + @nullcnt != @numrows
    BEGIN
      SELECT @errno  = 30007,
             @errmsg = 'Cannot update Aukcija because Bid does not exist.'
      GOTO error
    END
  END

  /* erwin Builtin Trigger */
  /* Korisnik  Aukcija on child update no action */
  /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_2", FK_COLUMNS="KorisnikID" */
  IF
    /* %ChildFK(" OR",UPDATE) */
    UPDATE(KorisnikID)
  BEGIN
    SELECT @nullcnt = 0
    SELECT @validcnt = count(*)
      FROM inserted,Korisnik
        WHERE
          /* %JoinFKPK(inserted,Korisnik) */
          inserted.KorisnikID = Korisnik.KorisnikID
    /* %NotnullFK(inserted," IS NULL","select @nullcnt = count(*) from inserted where"," AND") */
    select @nullcnt = count(*) from inserted where
      inserted.KorisnikID IS NULL
    IF @validcnt + @nullcnt != @numrows
    BEGIN
      SELECT @errno  = 30007,
             @errmsg = 'Cannot update Aukcija because Korisnik does not exist.'
      GOTO error
    END
  END


  /* erwin Builtin Trigger */
  RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go




CREATE TRIGGER tD_Bid ON Bid FOR DELETE AS
/* erwin Builtin Trigger */
/* DELETE trigger on Bid */
BEGIN
  DECLARE  @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)
    /* erwin Builtin Trigger */
    /* Bid  Aukcija on parent delete no action */
    /* ERWIN_RELATION:CHECKSUM="00032f47", PARENT_OWNER="", PARENT_TABLE="Bid"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_6", FK_COLUMNS="BidID" */
    IF EXISTS (
      SELECT * FROM deleted,Aukcija
      WHERE
        /*  %JoinFKPK(Aukcija,deleted," = "," AND") */
        Aukcija.BidID = deleted.BidID
    )
    BEGIN
      SELECT @errno  = 30001,
             @errmsg = 'Cannot delete Bid because Aukcija exists.'
      GOTO error
    END

    /* erwin Builtin Trigger */
    /* Korisnik  Bid on child delete no action */
    /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_4", FK_COLUMNS="KorisnikID" */
    IF EXISTS (SELECT * FROM deleted,Korisnik
      WHERE
        /* %JoinFKPK(deleted,Korisnik," = "," AND") */
        deleted.KorisnikID = Korisnik.KorisnikID AND
        NOT EXISTS (
          SELECT * FROM Bid
          WHERE
            /* %JoinFKPK(Bid,Korisnik," = "," AND") */
            Bid.KorisnikID = Korisnik.KorisnikID
        )
    )
    BEGIN
      SELECT @errno  = 30010,
             @errmsg = 'Cannot delete last Bid because Korisnik exists.'
      GOTO error
    END

    /* erwin Builtin Trigger */
    /* Aukcija  Bid on child delete no action */
    /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Aukcija"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_3", FK_COLUMNS="AukcijaID" */
    IF EXISTS (SELECT * FROM deleted,Aukcija
      WHERE
        /* %JoinFKPK(deleted,Aukcija," = "," AND") */
        deleted.AukcijaID = Aukcija.AukcijaID AND
        NOT EXISTS (
          SELECT * FROM Bid
          WHERE
            /* %JoinFKPK(Bid,Aukcija," = "," AND") */
            Bid.AukcijaID = Aukcija.AukcijaID
        )
    )
    BEGIN
      SELECT @errno  = 30010,
             @errmsg = 'Cannot delete last Bid because Aukcija exists.'
      GOTO error
    END


    /* erwin Builtin Trigger */
    RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go


CREATE TRIGGER tU_Bid ON Bid FOR UPDATE AS
/* erwin Builtin Trigger */
/* UPDATE trigger on Bid */
BEGIN
  DECLARE  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insBidID int,
           @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)

  SELECT @numrows = @@rowcount
  /* erwin Builtin Trigger */
  /* Bid  Aukcija on parent update no action */
  /* ERWIN_RELATION:CHECKSUM="0003d65b", PARENT_OWNER="", PARENT_TABLE="Bid"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_6", FK_COLUMNS="BidID" */
  IF
    /* %ParentPK(" OR",UPDATE) */
    UPDATE(BidID)
  BEGIN
    IF EXISTS (
      SELECT * FROM deleted,Aukcija
      WHERE
        /*  %JoinFKPK(Aukcija,deleted," = "," AND") */
        Aukcija.BidID = deleted.BidID
    )
    BEGIN
      SELECT @errno  = 30005,
             @errmsg = 'Cannot update Bid because Aukcija exists.'
      GOTO error
    END
  END

  /* erwin Builtin Trigger */
  /* Korisnik  Bid on child update no action */
  /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_4", FK_COLUMNS="KorisnikID" */
  IF
    /* %ChildFK(" OR",UPDATE) */
    UPDATE(KorisnikID)
  BEGIN
    SELECT @nullcnt = 0
    SELECT @validcnt = count(*)
      FROM inserted,Korisnik
        WHERE
          /* %JoinFKPK(inserted,Korisnik) */
          inserted.KorisnikID = Korisnik.KorisnikID
    /* %NotnullFK(inserted," IS NULL","select @nullcnt = count(*) from inserted where"," AND") */
    select @nullcnt = count(*) from inserted where
      inserted.KorisnikID IS NULL
    IF @validcnt + @nullcnt != @numrows
    BEGIN
      SELECT @errno  = 30007,
             @errmsg = 'Cannot update Bid because Korisnik does not exist.'
      GOTO error
    END
  END

  /* erwin Builtin Trigger */
  /* Aukcija  Bid on child update no action */
  /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Aukcija"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_3", FK_COLUMNS="AukcijaID" */
  IF
    /* %ChildFK(" OR",UPDATE) */
    UPDATE(AukcijaID)
  BEGIN
    SELECT @nullcnt = 0
    SELECT @validcnt = count(*)
      FROM inserted,Aukcija
        WHERE
          /* %JoinFKPK(inserted,Aukcija) */
          inserted.AukcijaID = Aukcija.AukcijaID
    /* %NotnullFK(inserted," IS NULL","select @nullcnt = count(*) from inserted where"," AND") */
    select @nullcnt = count(*) from inserted where
      inserted.AukcijaID IS NULL
    IF @validcnt + @nullcnt != @numrows
    BEGIN
      SELECT @errno  = 30007,
             @errmsg = 'Cannot update Bid because Aukcija does not exist.'
      GOTO error
    END
  END


  /* erwin Builtin Trigger */
  RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go




CREATE TRIGGER tD_Korisnik ON Korisnik FOR DELETE AS
/* erwin Builtin Trigger */
/* DELETE trigger on Korisnik */
BEGIN
  DECLARE  @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)
    /* erwin Builtin Trigger */
    /* Korisnik  Bid on parent delete no action */
    /* ERWIN_RELATION:CHECKSUM="0002cd6d", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_4", FK_COLUMNS="KorisnikID" */
    IF EXISTS (
      SELECT * FROM deleted,Bid
      WHERE
        /*  %JoinFKPK(Bid,deleted," = "," AND") */
        Bid.KorisnikID = deleted.KorisnikID
    )
    BEGIN
      SELECT @errno  = 30001,
             @errmsg = 'Cannot delete Korisnik because Bid exists.'
      GOTO error
    END

    /* erwin Builtin Trigger */
    /* Korisnik  Aukcija on parent delete no action */
    /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_2", FK_COLUMNS="KorisnikID" */
    IF EXISTS (
      SELECT * FROM deleted,Aukcija
      WHERE
        /*  %JoinFKPK(Aukcija,deleted," = "," AND") */
        Aukcija.KorisnikID = deleted.KorisnikID
    )
    BEGIN
      SELECT @errno  = 30001,
             @errmsg = 'Cannot delete Korisnik because Aukcija exists.'
      GOTO error
    END

    /* erwin Builtin Trigger */
    /* Korisnik  Narudzbina on parent delete no action */
    /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Narudzbina"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_1", FK_COLUMNS="KorisnikID" */
    IF EXISTS (
      SELECT * FROM deleted,Narudzbina
      WHERE
        /*  %JoinFKPK(Narudzbina,deleted," = "," AND") */
        Narudzbina.KorisnikID = deleted.KorisnikID
    )
    BEGIN
      SELECT @errno  = 30001,
             @errmsg = 'Cannot delete Korisnik because Narudzbina exists.'
      GOTO error
    END


    /* erwin Builtin Trigger */
    RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go


CREATE TRIGGER tU_Korisnik ON Korisnik FOR UPDATE AS
/* erwin Builtin Trigger */
/* UPDATE trigger on Korisnik */
BEGIN
  DECLARE  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insKorisnikID int,
           @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)

  SELECT @numrows = @@rowcount
  /* erwin Builtin Trigger */
  /* Korisnik  Bid on parent update no action */
  /* ERWIN_RELATION:CHECKSUM="0003327b", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Bid"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_4", FK_COLUMNS="KorisnikID" */
  IF
    /* %ParentPK(" OR",UPDATE) */
    UPDATE(KorisnikID)
  BEGIN
    IF EXISTS (
      SELECT * FROM deleted,Bid
      WHERE
        /*  %JoinFKPK(Bid,deleted," = "," AND") */
        Bid.KorisnikID = deleted.KorisnikID
    )
    BEGIN
      SELECT @errno  = 30005,
             @errmsg = 'Cannot update Korisnik because Bid exists.'
      GOTO error
    END
  END

  /* erwin Builtin Trigger */
  /* Korisnik  Aukcija on parent update no action */
  /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Aukcija"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_2", FK_COLUMNS="KorisnikID" */
  IF
    /* %ParentPK(" OR",UPDATE) */
    UPDATE(KorisnikID)
  BEGIN
    IF EXISTS (
      SELECT * FROM deleted,Aukcija
      WHERE
        /*  %JoinFKPK(Aukcija,deleted," = "," AND") */
        Aukcija.KorisnikID = deleted.KorisnikID
    )
    BEGIN
      SELECT @errno  = 30005,
             @errmsg = 'Cannot update Korisnik because Aukcija exists.'
      GOTO error
    END
  END

  /* erwin Builtin Trigger */
  /* Korisnik  Narudzbina on parent update no action */
  /* ERWIN_RELATION:CHECKSUM="00000000", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Narudzbina"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_1", FK_COLUMNS="KorisnikID" */
  IF
    /* %ParentPK(" OR",UPDATE) */
    UPDATE(KorisnikID)
  BEGIN
    IF EXISTS (
      SELECT * FROM deleted,Narudzbina
      WHERE
        /*  %JoinFKPK(Narudzbina,deleted," = "," AND") */
        Narudzbina.KorisnikID = deleted.KorisnikID
    )
    BEGIN
      SELECT @errno  = 30005,
             @errmsg = 'Cannot update Korisnik because Narudzbina exists.'
      GOTO error
    END
  END


  /* erwin Builtin Trigger */
  RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go




CREATE TRIGGER tD_Narudzbina ON Narudzbina FOR DELETE AS
/* erwin Builtin Trigger */
/* DELETE trigger on Narudzbina */
BEGIN
  DECLARE  @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)
    /* erwin Builtin Trigger */
    /* Korisnik  Narudzbina on child delete no action */
    /* ERWIN_RELATION:CHECKSUM="00014ad3", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Narudzbina"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_1", FK_COLUMNS="KorisnikID" */
    IF EXISTS (SELECT * FROM deleted,Korisnik
      WHERE
        /* %JoinFKPK(deleted,Korisnik," = "," AND") */
        deleted.KorisnikID = Korisnik.KorisnikID AND
        NOT EXISTS (
          SELECT * FROM Narudzbina
          WHERE
            /* %JoinFKPK(Narudzbina,Korisnik," = "," AND") */
            Narudzbina.KorisnikID = Korisnik.KorisnikID
        )
    )
    BEGIN
      SELECT @errno  = 30010,
             @errmsg = 'Cannot delete last Narudzbina because Korisnik exists.'
      GOTO error
    END


    /* erwin Builtin Trigger */
    RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go


CREATE TRIGGER tU_Narudzbina ON Narudzbina FOR UPDATE AS
/* erwin Builtin Trigger */
/* UPDATE trigger on Narudzbina */
BEGIN
  DECLARE  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insNarudzbinaID int,
           @errno   int,
           @severity int,
           @state    int,
           @errmsg  varchar(255)

  SELECT @numrows = @@rowcount
  /* erwin Builtin Trigger */
  /* Korisnik  Narudzbina on child update no action */
  /* ERWIN_RELATION:CHECKSUM="00018f88", PARENT_OWNER="", PARENT_TABLE="Korisnik"
    CHILD_OWNER="", CHILD_TABLE="Narudzbina"
    P2C_VERB_PHRASE="", C2P_VERB_PHRASE="", 
    FK_CONSTRAINT="R_1", FK_COLUMNS="KorisnikID" */
  IF
    /* %ChildFK(" OR",UPDATE) */
    UPDATE(KorisnikID)
  BEGIN
    SELECT @nullcnt = 0
    SELECT @validcnt = count(*)
      FROM inserted,Korisnik
        WHERE
          /* %JoinFKPK(inserted,Korisnik) */
          inserted.KorisnikID = Korisnik.KorisnikID
    /* %NotnullFK(inserted," IS NULL","select @nullcnt = count(*) from inserted where"," AND") */
    select @nullcnt = count(*) from inserted where
      inserted.KorisnikID IS NULL
    IF @validcnt + @nullcnt != @numrows
    BEGIN
      SELECT @errno  = 30007,
             @errmsg = 'Cannot update Narudzbina because Korisnik does not exist.'
      GOTO error
    END
  END


  /* erwin Builtin Trigger */
  RETURN
error:
   RAISERROR (@errmsg, -- Message text.
              @severity, -- Severity (0~25).
              @state) -- State (0~255).
    rollback transaction
END

go






