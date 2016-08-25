import fetch from "universal-fetch";

/**
 * Realiza um pedido http ao URL fornecido, deixando
 * o resultado no callback passado por parametro ou
 * uma instãncia de Error caso ocorra.
 *
 * cb : (err, res) => void
 */
export function request(url, cb){
	let self = this;
	fetch(url)
		.then(response => {
			if (response.status >= 400) {
				//TODO
				console.log("ERROR:\n" + response.body);
				throw new Error(response.body);
			}
			return response.json();
		})
		.then(res => {
			cb(null, res);
		})
		.catch(err => {
			console.log(err);
			cb(err);
		});
}


export function createNewElement(url, template, cb){
//post to collection uri
	fetch(url, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/vnd.collection+json'
		},
			body: JSON.stringify(template)
	})
	.then(res => res.json())
	.then(res => {
		if (cb)cb(res)
	})
	.catch(err => console.log("Network failure. " + err.message));
}


function checkStatus(res) {
  if (res.status >= 200 && res.status < 300) {
    return res.json();
  } else {
  	console.log("Request sent was not completed.");
  	return res.body.json();
  }
}

