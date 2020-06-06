FROM python:3.8

# copy all the files to the container
# TODO: idk wht, but with docker-compose nothing is copied
COPY . .

# install python dependencies
RUN pip install -r ./Server/req.txt
