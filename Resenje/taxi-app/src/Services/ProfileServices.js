import axios from "axios";
import { SHA256 } from 'crypto-js';
import qs from 'qs';
//This function make image for display on page 
export function makeImage(imageFile) { //prima imageFile koji sadrzi sve ono sto smo na serveru za fajl-sliku definisali
    console.log("Usao");
    if (imageFile.fileContent) {
        const byteCharacters = atob(imageFile.fileContent); //niz bajtova
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers); //dekodira ga u Uint8Array
        const blob = new Blob([byteArray], { type: imageFile.contentType }); //kreira blob
        const url = URL.createObjectURL(blob); //prikaz slike u komponenti
        return url;
    }
}
export function convertDateTimeToDateOnly(dateTime) { //prima datum u formatu DateTime pa ga parsita i vraca u DD-MM-YYYY
    const dateObj = new Date(dateTime);

    // Get the date components
    const year = dateObj.getFullYear();
    const month = dateObj.getMonth();
    const day = dateObj.getDate();

    // Format the date as 'DD-MM-YYYY'
    return `${day.toString().padStart(2, '0')}-${(month + 1).toString().padStart(2, '0')}-${year}`;
}


export async function changeUserFields(apiEndpoint, firstName, lastName, birthday, address, email, password, imageUrl, username, jwt, newPassword, newPasswordRepeat, oldPasswordRepeat,id) {
    const formData = new FormData(); //formData sa svim potrebnim informacijama za azuriranje korisnika
    formData.append('FirstName', firstName);
    formData.append('LastName', lastName);
    formData.append('Birthday', birthday);
    formData.append('Address', address);
    formData.append('Email', email);
    formData.append("Id",id);
    if(newPassword!='') { //ako je unesena nova lozinka ona se opet mora hesovat
        const hashPw = SHA256(newPassword).toString();
        formData.append('Password', hashPw);
    }
    else{ 
    const hashPw = '';
    formData.append('Password', hashPw);
    }
    formData.append('ImageUrl', imageUrl);
    formData.append('Username', username);

    if(oldPasswordRepeat!='' || newPasswordRepeat!='' || newPassword!='' && checkNewPassword(password,oldPasswordRepeat,newPassword,newPasswordRepeat)){
            console.log("Succesfully entered new passwords");
            const dataOtherCall = await changeUserFieldsApiCall(apiEndpoint,formData,jwt); //na server salje
            console.log("data other call",dataOtherCall);
            return dataOtherCall.changedUser; //vraca korisnika sa azuirranim poljima
    }else if(oldPasswordRepeat=='' && newPasswordRepeat=='' && newPassword==''){ //ako nista nije mijenjao od sifri korisnik nema potrebe za provjerom lozinki sa funkcijom
        const data = await changeUserFieldsApiCall(apiEndpoint,formData,jwt);
        return data.changedUser; //isto
    }

    
}


export async function getUserInfo(jwt, apiEndpoint, userId) {
    try {
        const config = { //JWT token se preuzima iz zaglavlja za autentifikaciju
            headers: { //Ovo zaglavlje šalje JWT token serveru za autentifikaciju korisnika.
                Authorization: `Bearer ${jwt}`,
                'Content-Type': 'application/json'
            }
        };

        const queryParams = qs.stringify({ Id: userId });

        const url = `${apiEndpoint}?${queryParams}`; //ovako salje id da bih znao za koji da iscitam informacije
        const response = await axios.get(url, config); //get
        return response.data; //inf o korisniku
    } catch (error) {
        console.error('Error fetching data (async/await):', error.message);
        throw error;
    }
}



export async function changeUserFieldsApiCall(apiEndpoint,formData,jwt){ //put metoda za izmenu polja
    try {
        const response = await axios.put(apiEndpoint, formData, {
            headers: {
                'Authorization': `Bearer ${jwt}`, //JWT token standardno
                'Content-Type': 'multipart/form-data'
            }
        
        });
        return response.data; //vraca azuriranog korisnika
    } catch (error) {
        console.error('Error while calling API to change user fields:', error);
    }
}

/**
 Oldpassword is hashed value from database
 OldPasswordRepeat is string need to be hashed 
 NewPassword and newPasswordRepeat are string need to be hashed
 newPassword, newPasswordRepeat, oldPassword, oldPasswordRepeat
 */
//u sustini ako lozina nije promenjena provjerava se da li su stare lozinke iste, a ako je unesena nova lzonika provjerava da li su nove lozinke iste. Mora se uneti stara lozinka koja ce se provjerit sa lozinkom u bazi da bi korisnik mogao tek onda uneti novu
export function checkNewPassword(oldPassword,oldPasswordRepeat,newPassword,newPasswordRepeat) {
    const hashedPassword = SHA256(oldPasswordRepeat).toString();
    if (oldPassword != hashedPassword) {
        alert("Old Password You Entered Was Incorrect");
        return false;
    }
    const newPasswordhash = SHA256(newPassword).toString();
    const newPasswordhashRepeatHash = SHA256(newPasswordRepeat).toString();
    if (newPasswordhash == newPasswordhashRepeatHash) return true;
    else {
        alert("New Passwords do NOT match");
        return false;
    }
}