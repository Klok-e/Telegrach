from sqlalchemy import create_engine
import config
import models

if __name__ == "__main__":
    engine = create_engine(config.connect_string(), echo=True)
    models.Base.metadata.create_all(engine)
