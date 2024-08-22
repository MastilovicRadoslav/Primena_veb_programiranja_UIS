import axios from "axios";
import { SHA256 } from 'crypto-js';


export async function LoginApiCall(email, password, apiEndpoint) { //poslat je email, password i api za Login funkciju na bekendu
    try {
        const response = await axios.post(apiEndpoint, {
            email: email,
            password: SHA256(password).toString() //salje se hesirana lozinka
        });
        return response.data; //funkcija vraca JWT token i korisnicke informacije
    } catch (error) {
        console.error('Error while calling api for login user:', error);
    }
}

