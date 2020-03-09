import binascii
import hashlib
import os
 
def HashPassword(password: str):
    '''Hash a given password.'''
    salt = binascii.hexlify(os.urandom(16))
    print(salt, type(salt))
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




