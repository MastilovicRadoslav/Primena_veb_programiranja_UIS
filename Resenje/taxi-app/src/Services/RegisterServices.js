import axios from "axios";
import { SHA256 } from 'crypto-js'; //hesiranje lozinke prije slanja na server
export function DropErrorForField(firstNameError, lastNameError, birthdayError, addressError, usernameError, emailError, passwordError, repeatPasswordError, imageUrlError) {
    let isValid = true;

    if (firstNameError) {
        alert("First Name is required");
        isValid = false;
    } else if (lastNameError) {
        alert("Last Name is required");
        isValid = false;
    } else if (birthdayError) {
        alert("You must be older than 18");
        isValid = false;
    } else if (addressError) {
        alert("Address is required");
        isValid = false;
    } else if (usernameError) {
        alert("Username is required");
        isValid = false;
    } else if (emailError) {
        alert("This is not a valid email!");
        isValid = false;
    } else if (passwordError || repeatPasswordError) {
        alert("Passwords do NOT match!");
        isValid = false;
    } else if (imageUrlError) {
        alert("Image is required");
        isValid = false;
    }

    return isValid;
}

export async function RegularRegisterApiCall(

    firstNameError,
    lastNameError,
    birthdayError,
    addressError,
    usernameError,
    emailError,
    passwordError,
    repeatPasswordError,
    imageUrlError,
    firstName,
    lastName,
    birthday,
    address,
    email,
    password,
    repeatPassword,
    imageUrl,
    typeOfUser,
    username,
    apiEndpoint
) {

    const isCompleted = DropErrorForField(
        firstNameError,
        lastNameError,
        birthdayError,
        addressError,
        usernameError,
        emailError,
        passwordError,
        repeatPasswordError,
        imageUrlError
    );
    if (isCompleted) { //ako nema gresaka
        const formData = new FormData(); //slanje podataka
        formData.append('firstName', firstName);
        formData.append('lastName', lastName);
        formData.append('birthday', birthday);
        formData.append('address', address);
        formData.append('email', email);
        const hashedPassword = SHA256(password).toString(); //hesovanje
        formData.append('password', hashedPassword);
        formData.append('typeOfUser', typeOfUser);
        formData.append('username', username);
        formData.append('imageUrl', imageUrl);
        //da li su hesirane vrednosti lozinke i ponovljene lozinke iste
        if (formData.get("password") === SHA256(repeatPassword).toString()) {
            try {
                const response = await axios.post(apiEndpoint, formData, { //slanje POST zahteva
                    headers: {
                        'Content-Type': 'multipart/form-data' //salju se fajlovi (slika se konkretno salje)
                    }
                 
                });
                return true;
            } catch (error) {
                if(error.response.status == 409){ //to je greska 409
                    alert(error.response.data+"\nTry another email!!!");
                }
            }
        }
    }
}



