from sqlalchemy import create_engine
import config
import models

if __name__ == "__main__":
    engine = create_engine(config.get_connect_string(), echo=True)
    models.metadata.create_all(engine)
    models.metadata.drop_all(engine)
