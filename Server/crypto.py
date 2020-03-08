'''
    Part used for handling hashes, encodes and passwords.
'''
import binascii
import secrets
import hashlib
import string
import os


def GeneratePassword():
    ''' Just generating password'''
    alphabet = string.ascii_letters + string.digits
    password = ''.join(secrets.choice(alphabet) for i in range(10))
    return password

def HashPassword(password: str):
    '''Hash a given password.'''
    salt = binascii.hexlify(os.urandom(16))
    hashed_password = hashlib.pbkdf2_hmac("sha256", password.encode(),
                                          salt, 10000, 64)
    salt = salt.decode()
    hashed_password = binascii.hexlify(hashed_password).decode()
    return salt, hashed_password
 
def VerifyPassword(stored_salt, stored_password, provided_password):
    '''Verify a stored password'''
    hashed_password = hashlib.pbkdf2_hmac("sha256", provided_password.encode(),
                                          stored_salt.encode(), 10000, 64)
    hashed_password = binascii.hexlify(hashed_password).decode()
    return hashed_password == stored_password




