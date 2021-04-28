function base64Decode(base64) {
    return Uint8Array.from(atob(base64), c => c.charCodeAt(0));
}

async function makeBase64AnswerOrError(arrayPromise) {
    try {
        var result = await arrayPromise;
        var base64Result = btoa(String.fromCharCode.apply(null, new Uint8Array(result)));
        return { a: base64Result };
    } catch (error) {
        return { e: error.toString() };
    }
}

export async function computeDigest(algorithm, data) {
    var parsedData = base64Decode(data);
    var promise = window.crypto.subtle.digest(algorithm, parsedData);
    return await makeBase64AnswerOrError(promise);
}

var keys = {};

export function freeKey(keyId) {
    delete keys[keyId];
}

export async function generateSecretKey(keySize, keyId, algorithm) {
    if (keys[keyId]) {
        return null;
    }

    var genAlg = { name: algorithm, length: keySize };
    var cryptoKey = await window.crypto.subtle.generateKey(genAlg, true, ["encrypt", "decrypt"]);

    if (cryptoKey) {
        keys[keyId] = cryptoKey;
        return true;
    }

    return false;
}

export async function importSecretKey(key, keyId, algorithm) {
    if (keys[keyId]) {
        return null;
    }

    var parsedKey = base64Decode(key);
    var cryptoKey = await window.crypto.subtle.importKey("raw", parsedKey, algorithm, true, ["encrypt", "decrypt"]);

    if (cryptoKey) {
        keys[keyId] = cryptoKey;
        return true;
    }

    return false;
}

export async function exportSecretKey(keyId) {
    var cryptoKey = keys[keyId];
    var promise = window.crypto.subtle.exportKey("raw", cryptoKey);
    return await makeBase64AnswerOrError(promise);
}

export async function symmetricEncrypt(keyId, data, iv, algorithm) {
    var parsedData = base64Decode(data);
    var alg = { name: algorithm, iv: base64Decode(iv) };
    var cryptoKey = keys[keyId];
    var promise = window.crypto.subtle.encrypt(alg, cryptoKey, parsedData);
    return await makeBase64AnswerOrError(promise);
}

export async function symmetricDecrypt(keyId, data, iv, algorithm) {
    var parsedData = base64Decode(data);
    var alg = { name: algorithm, iv: base64Decode(iv) };
    var cryptoKey = keys[keyId];
    var promise = window.crypto.subtle.decrypt(alg, cryptoKey, parsedData);
    return await makeBase64AnswerOrError(promise);
}
