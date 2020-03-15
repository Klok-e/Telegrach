from sqlalchemy import create_engine
import config
import models

if __name__ == "__main__":
    engine = create_engine(f'postgresql://{config.DB_USER}:{config.DB_PW}@{config.DB_HOST}/{config.SCHEMA_NAME}',
                           echo=True)
    models.metadata.create_all(engine)
