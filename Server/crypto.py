'''
    Part used for handling hashes, encodes and passwords.
'''
import binascii
import secrets
import hashlib
import base64
import string
import os
from typing import Tuple
from Crypto.Util import Counter
from Crypto.Cipher import AES
from Crypto import Random


# # AES supports multiple key sizes: 16 (AES128), 24 (AES192), or 32 (AES256).
# key_bytes = 32

# # Takes as input a 32-byte key and an arbitrary-length plaintext and returns a
# # pair (iv, ciphtertext). "iv" stands for initialization vector.
# def encrypt(key, plaintext):
#     assert len(key) == key_bytes

#     # Choose a random, 16-byte IV.
#     iv = Random.new().read(AES.block_size)

#     # Convert the IV to a Python integer.
#     iv_int = int(binascii.hexlify(iv), 16)

#     # Create a new Counter object with IV = iv_int.
#     ctr = Counter.new(AES.block_size * 8, initial_value=iv_int)

#     # Create AES-CTR cipher.
#     aes = AES.new(key, AES.MODE_CTR, counter=ctr)

#     # Encrypt and return IV and ciphertext.
#     ciphertext = aes.encrypt(plaintext)
#     return (iv, ciphertext)

# # Takes as input a 32-byte key, a 16-byte IV, and a ciphertext, and outputs the
# # corresponding plaintext.
# def decrypt(key, iv, ciphertext):
#     assert len(key) == key_bytes

#     # Initialize counter for decryption. iv should be the same as the output of
#     # encrypt().
#     iv_int = int(iv.encode('hex'), 16)
#     ctr = Counter.new(AES.block_size * 8, initial_value=iv_int)

#     # Create AES-CTR cipher.
#     aes = AES.new(key, AES.MODE_CTR, counter=ctr)

#     # Decrypt and return the plaintext.
#     plaintext = aes.decrypt(ciphertext)
#     return plaintext

# (iv, ciphertext) = encrypt(key, 'hella')
# print decrypt(key, iv, ciphertext)


def generate_password() -> str:
    ''' Just generating password'''
    alphabet = string.ascii_letters + string.digits
    password = ''.join(secrets.choice(alphabet) for i in range(10))
    return password


def hash_password(password: str) -> Tuple[str, str]:
    '''Hash a given password.'''
    salt = binascii.hexlify(os.urandom(16))
    hashed_password = hashlib.pbkdf2_hmac("sha256", password.encode(),
                                          salt, 10000, 64)
    salt = salt.decode()
    hashed_password = binascii.hexlify(hashed_password).decode()
    return salt, hashed_password


def validate_password(stored_salt: str, stored_password: str, provided_password: str) -> bool:
    '''Verify a stored password'''
    hashed_password = hashlib.pbkdf2_hmac("sha256", provided_password.encode(),
                                          stored_salt.encode(), 10000, 64)
    hashed_password = binascii.hexlify(hashed_password).decode()
    return hashed_password == stored_password
