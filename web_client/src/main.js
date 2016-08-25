import React from "react";
import ReactDOM from "react-dom";
import fetch from "universal-fetch";

import ApplicationWrapper from './components/ApplicationWrapperComponent';
import Parser from './ApiParser';

let app = document.getElementById("application");
let mainUrl = "/api/";


function renderApp(app, parser) {
  ReactDOM.render( <div>
    < ApplicationWrapper parser = {
      parser
    }
    /> </div> , app);
}

fetch(mainUrl)
  .then(function (response) {
    if (response.status >= 400) {
      //TODO 
      throw new Error("Bad response from server");
    }
    return response.json();
  })
  .then(function (resp) {
    renderApp(app, new Parser(resp));
  })
  .catch(err => {
    console.log(err.message);
  });
