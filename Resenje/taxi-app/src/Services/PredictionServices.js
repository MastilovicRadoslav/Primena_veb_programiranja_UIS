import axios from "axios";
import qs from 'qs';
//GetPredictionPrice, convertTimeStringToMinutes
export async function getEstimation(jwt, apiEndpoint, currentLocation,Destination) { //procena cenje voznje na osnovu trenutne lokacije i destinacije
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`, //JWT token za autorizaciju standardno
                'Content-Type': 'application/json' //Tip sadrzaja
            }
        };
        //Priprema parametara upita u URL fomratu (query string)
        const queryParams = qs.stringify({ Destination: Destination,CurrentLocation:currentLocation }); 
        //Kompletan URL sa parametrima
        const url = `${apiEndpoint}?${queryParams}`;
        //Slanje GET zahteva sa konfiguracijom zaglavlja
        const response = await axios.get(url, config);
        //Vraceni podaci iz odgovora servera
        return response.data;
    } catch (error) {
        //Obrada greske
        console.error('Error fetching data (async/await):', error.message);
        throw error;
    }
}

//AcceptSuggestedDrive
export async function AcceptDrive(apiEndpoint, id, jwt,currentLocation,destination,price,isAccepted,estimatedTimeDriverArrival) { //Kada korisnik prihvati predlozenu voznju saljem je na upis na bekend u bazu i Reliable
    try {
        //Priprema podataka za slanje u telu zahteva
        const response = await axios.put(apiEndpoint, {
            RiderId: id, //ID korisnika
            Destination: destination, //Destinacija
            CurrentLocation : currentLocation, //Trenutna lokacija
            Price : price, //Cena voznje
            isAccepted : isAccepted, //Da li je voznja prihvacena
            MinutesToDriverArrive : estimatedTimeDriverArrival //Procena vremena dolaska vozaca
        }, {
            headers: {
                Authorization: `Bearer ${jwt}` //   JWT token za autorizaciju
            }
        });
        //Prikazuje odgovor u konzoli da proverim
        console.log("This is response",response);
        //Vraceni podaci kao odgovor od servera
        return response.data;
    } catch (error) {
        //Obrada greske
        console.error('Error while calling api for login user:', error);
        return error;
    }
}
//Samo za fronted da konvertujem vreme u formatu HH:MM::SS u ukuopan broj minuta
export function convertTimeStringToMinutes(timeString) {
    // Razdvaja string na delove (sati, minuti, sekunde)
    const parts = timeString.split(':');
    
     // Ekstrahuje sate, minute i sekunde i konvertuje ih u brojeve
    const hours = parseInt(parts[0], 10);
    const minutes = parseInt(parts[1], 10);
    const seconds = parseInt(parts[2], 10);
    
    // Konvertuje vreme u ukupan broj minuta
    const totalMinutes = (hours * 60) + minutes + (seconds / 60);
    
    // Vraća ukupan broj minuta
    return totalMinutes;
}
