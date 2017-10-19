from sqlalchemy import (
    Column,
    Index,
    Integer,
    String,
    Text,
    Date,
)

from .meta import Base


class MyModel(Base):
    __tablename__ = 'models'
    id = Column(Integer, primary_key=True)
    name = Column(Text)
    value = Column(Integer)


Index('my_index', MyModel.name, unique=True, mysql_length=255)


class Member(Base):
    __tablename__ = 'member'
    id_member = Column(Integer, autoincrement=True, primary_key=True)
    nis_member  =  Column(String(20))
    surname_member  =  Column(String(20))
    name_member  =  Column(String(20))
    birth_date_member  =  Column(Date)
    birth_place_member  =  Column(String(50))
    birth_prov_member  =  Column(String(2))
    fiscal_code_member  =  Column(String(16))
    res_addr_member  =  Column(String(100))
    res_place_member  =  Column(String(50))
    res_prov_member  =  Column(String(2))

