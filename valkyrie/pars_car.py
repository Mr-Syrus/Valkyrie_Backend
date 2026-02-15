import csv
from sqlalchemy import create_engine, Column, Integer, String, ForeignKey
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker, relationship
from enum import Enum as PyEnum

# --------------------
# Настройки подключения к PostgreSQL
# --------------------
DATABASE_URL = "postgresql+psycopg2://postgres:111@localhost:5432/valkyrie"
engine = create_engine(DATABASE_URL)
Session = sessionmaker(bind=engine)
session = Session()
Base = declarative_base()

# --------------------
# Enum для топлива
# --------------------
class FuelType(PyEnum):
    Petrol = 0
    Diesel = 1
    Gas = 2
    Hydrogen = 3
    Electricity = 4

# --------------------
# Таблицы
# --------------------
class CarBrand(Base):
    __tablename__ = "CarBrands"
    id = Column(Integer, primary_key=True)
    name = Column(String(50), unique=True, nullable=False)

class CarType(Base):
    __tablename__ = "CarTypes"
    id = Column(Integer, primary_key=True)
    name = Column(String(50), unique=True, nullable=False)

class ModelCar(Base):
    __tablename__ = "ModelCars"
    id = Column(Integer, primary_key=True)
    fuel_type = Column(Integer, nullable=False)
    car_brand_id = Column(Integer, ForeignKey("CarBrands.id"), nullable=False)
    car_brand = relationship("CarBrand", backref="models")
    car_type_id = Column(Integer, ForeignKey("CarTypes.id"), nullable=False)
    car_type = relationship("CarType", backref="models")
    year_release = Column(Integer, nullable=False)

# --------------------
# Функция для сопоставления топлива
# --------------------
def map_fuel(fuel_str):
    fuel_str = (fuel_str or "").lower()
    if "petrol" in fuel_str or "gasoline" in fuel_str:
        return FuelType.Petrol.value
    elif "diesel" in fuel_str:
        return FuelType.Diesel.value
    elif "gas" in fuel_str:
        return FuelType.Gas.value
    elif "hydrogen" in fuel_str:
        return FuelType.Hydrogen.value
    elif "electric" in fuel_str:
        return FuelType.Electricity.value
    else:
        return FuelType.Petrol.value  # дефолт

# --------------------
# Чтение CSV и заполнение БД
# --------------------
CSV_FILE = "Car_Specification_1945_2020.csv"

from tqdm import tqdm

with open(CSV_FILE, newline='', encoding='utf-8') as csvfile:
    reader = csv.DictReader(csvfile)
    for row in tqdm(reader, desc="Импорт данных"):
        make = row.get("Make", "").strip()
        model = row.get("Modle", "").strip()  # обращаем внимание на опечатку Modle
        year = row.get("Year_from", "").strip()
        body = row.get("Body_type", "").strip()
        fuel = row.get("fuel_grade", "").strip()

        if not (make and model and year and body):
            continue

        # CarBrand
        car_brand = session.query(CarBrand).filter_by(name=make).first()
        if not car_brand:
            car_brand = CarBrand(name=make)
            session.add(car_brand)
            session.commit()

        # CarType
        car_type = session.query(CarType).filter_by(name=body).first()
        if not car_type:
            car_type = CarType(name=body)
            session.add(car_type)
            session.commit()

        # ModelCar
        try:
            year_int = int(year)
        except:
            year_int = 0

        fuel_enum = map_fuel(fuel)

        model_car = ModelCar(
            fuel_type=fuel_enum,
            car_brand_id=car_brand.id,
            car_type_id=car_type.id,
            year_release=year_int
        )
        session.add(model_car)

    session.commit()
    print("Импорт CSV завершён")
