import React from 'react';

/**
* Este componente representa um formulário de pesquisa
* que é construido com base numa querie fornecida por respostas
* da API com mediatype collection+JSON.
*/
export default class SearchBox extends React.Component{

	constructor(props){
		super(props);
		this.state = {
		}
	}

	_handleOnSubmit(evt){
		evt.preventDefault();

		let queryPath = ""
		for(let p in this.state){
			queryPath += (this.state[p].length === 0) ? ''
			           :`&${p}=${this.state[p]}`;
		}

		//if empty, get all items
		let url = this.props.queryNode.href;
		url += (queryPath.length === 0) ? "" : queryPath;

		//console.log(url)
		this.props.onSearch(url);
	}

	_renderSearchBox(obj, idx){
		return (<div key={idx} className="form-group">
			<div className="input-group">
					<span className="input-group-addon">
							<i className="glyphicon glyphicon-search"></i>
					</span>
			<input
				name={obj.name}
				type="text"
				onChange={
					evt => this.setState({[obj.name]: evt.target.value})
				}
				className="form-control"
				placeholder="Search"/>
			</div>
		</div>)
	}


	render(){
		return (
			<div className="text-right">
				<form onSubmit={this._handleOnSubmit.bind(this)}
					className="navbar-form navbar-input-group" role="search">
					<p>{this.props.queryNode.prompt}</p>

					{this.props.queryNode.data.map(this._renderSearchBox.bind(this))}

				</form>
			</div>
			);
	}
}
