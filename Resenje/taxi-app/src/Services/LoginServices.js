import axios from "axios";
import { SHA256 } from 'crypto-js';


export async function LoginApiCall(email, password, apiEndpoint) {
    try {
        const response = await axios.post(apiEndpoint, {
            email: email,
            password: SHA256(password).toString()
        });
        return response.data;
    } catch (error) {
        console.error('Error while calling api for login user:', error);
    }
}

