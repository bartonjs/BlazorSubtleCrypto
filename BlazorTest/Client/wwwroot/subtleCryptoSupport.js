function base64Decode(base64) {
    return Uint8Array.from(atob(base64), c => c.charCodeAt(0));
}

function makeAnswer(value) {
    return { a: value };
}

function makeError(value) {
    return { e: value };
}

var keys = {};

export function freeKey(keyId) {
    delete keys[keyId];
}

export async function importSecretKey(key, keyId, algorithm) {
    if (keys[keyId]) {
        return null;
    }

    var parsedKey = base64Decode(key);
    var cryptoKey = await window.crypto.subtle.importKey("raw", parsedKey, algorithm, false, ["encrypt", "decrypt"]);

    if (cryptoKey) {
        keys[keyId] = cryptoKey;
        return true;
    }

    return false;
}

export async function symmetricEncrypt(keyId, data, iv, algorithm) {
    var parsedData = base64Decode(data);
    var alg = { name: algorithm, iv: base64Decode(iv) };
    var cryptoKey = keys[keyId];
    var answer = await window.crypto.subtle.encrypt(alg, cryptoKey, parsedData);
    return btoa(String.fromCharCode.apply(null, new Uint8Array(answer)));
}

export async function symmetricDecrypt(keyId, data, iv, algorithm) {
    var parsedData = base64Decode(data);
    var alg = { name: algorithm, iv: base64Decode(iv) };
    var cryptoKey = keys[keyId];
    try {
        var result = await window.crypto.subtle.decrypt(alg, cryptoKey, parsedData);
        var base64Result = btoa(String.fromCharCode.apply(null, new Uint8Array(result)));
        return makeAnswer(base64Result);
    } catch (error) {
        return makeError(error);
    }
}
