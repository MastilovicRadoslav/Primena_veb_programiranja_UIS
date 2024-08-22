import axios from 'axios';

export async function GetAllDrivers(apiEndpoint, jwtToken) { //Drivers.jsx
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwtToken}`, // JWT token se salje u HTTP zaglavlju 
            }
        };

        return axios.get(apiEndpoint, config)
        .then(response => response.data); //svi podaci o vozacima
        
    } catch (error) {
        console.error('Error fetching data (async/await):', error.message);
        throw error; 
    }
}

export async function GetDriversForVerify(apiEndpoint, jwtToken) { //dohvatanje svih vozaca za njihovu verifikaciju
    console.log(apiEndpoint);
    console.log(jwtToken);
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwtToken}`, // standardno saljem JWT token u zaglavlju zahteva radi autorizacije
            }
        };

        return axios.get(apiEndpoint, config) //vracam podatke o svim vozacima koji se tamo prikazuju
        .then(response => response.data);
        
    } catch (error) {
        console.error('Error fetching data (async/await):', error.message);
        throw error; // rethrow the error to handle it in the component
    }
}



export async function ChangeDriverStatus(apiEndpoint, id, changeStatus, jwt) { //Drivers.jsx
    try {
        const response = await axios.put(apiEndpoint, { //put za promenu statusa vozaca
            id: id,
            status: changeStatus
        }, {
            headers: {
                Authorization: `Bearer ${jwt}` //JWT token se salje u zaglavlju kako bi se osigurala autorizacija zahteva, iznad metode su definisane uloge
            }
        });
        console.log("Change of driver status sucesfully hapaned!",response);
    } catch (error) {
        console.error('Error while calling api change driver status:', error);
    }
}

export async function VerifyDriver(apiEndpoint, id, status,email, jwt) { //zahtev za promenu vozaca "prihvacen" ili  "pdbijen"
    try {
        const response = await axios.put(apiEndpoint, {
            Id: id,
            Action: status,
            Email : email
        }, {
            headers: {
                Authorization: `Bearer ${jwt}` //standardno za JWT token
            }
        });
        console.log("Driver verification sucessfuly!",response);
    } catch (error) {
        console.error('Error while calling api VerifyDriver:', error);
    }
}

export async function getAllRidesAdmin(jwt, apiEndpoint) { //Dobavlja sve voznje
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`, // JWT token za autorizaciju
                'Content-Type': 'application/json' // Postavljanje tipa sadržaja na JSON
            }
        };
        console.log(apiEndpoint);

        const response = await axios.get(apiEndpoint, config); // Slanje GET zahteva na backend
        return response.data; // Vraćanje podataka sa servera
    } catch (error) {
        return { error: error.response }; // Vraćanje greške u slučaju neuspeha
    }
}




