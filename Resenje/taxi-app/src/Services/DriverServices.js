import axios from "axios";
import qs from 'qs';
//GetAllUncompletedRides
export  async function getAllAvailableRides(jwt, apiEndpoint) { //dobavlja sve dostupne voznje koje vozac moze da prihvati
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`, //JWS token standardno u zaglavlju
                'Content-Type': 'application/json' //samo za POST i PUT zahteve
            }
        };
        const url = `${apiEndpoint}`;
        const response = await axios.get(url, config);//GET zahtev za dobijanje voznji
        return response.data; //odgovor
    } catch (error) {
        console.error('Error fetching data (async/await):', error.message);
        throw error;
    }
}

//AcceptNewRide
export  async function AcceptDrive(apiEndpoint, driverId,idRide,jwt) { //omogucava vozacu da prihvati voznju iz liste ponudjenih voznji
    try {
        
        const response = await axios.put(apiEndpoint, {
            DriverId :driverId, //Id vozaca koji prihvata voznju
            RideId :idRide ////Id voznje koja se prihvata
        }, {
            headers: {
                Authorization: `Bearer ${jwt}`
            }
        });

        return response.data;
    } catch (error) {
        console.error('Error while calling api for login user:', error);
        return error;
    }
}
//GetCurrentTripDriver
export async function getCurrentRide(jwt, apiEndpoint,userId) { //dobavljanje trenutne voznje ako postoji zajednicko za vozaca i korisnika voznje
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`,
                'Content-Type': 'application/json'
            }
        };
        console.log(apiEndpoint);
        const queryParams = qs.stringify({ id: userId });

        const url = `${apiEndpoint}?${queryParams}`;
        const response = await axios.get(url, config);
        return response.data;
    } catch (error) {
        return { error: error.response };
    }
}

export async function getMyRidesDriver(jwt, apiEndpoint,userId) { //Dobavljanje svih voznji za vozaca
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`,
                'Content-Type': 'application/json'
            }
        };
        console.log(apiEndpoint);
        const queryParams = qs.stringify({ id: userId });

        const url = `${apiEndpoint}?${queryParams}`;
        const response = await axios.get(url, config);
        return response.data;
    } catch (error) {
        //console.error('Error fetching data (async/await):', error.message);
        //throw error;
        return { error: error.response };
    }
}