import React from "react";

/**
* Para uma dada lista de elementos e uma função de mapeamento,
* este componente disponibiliza uma view com esses elementos após
* transformação via função de mapeamento.
*
* Caso a lista esteja vazia, o componente apenas apresenta uma mensagem a
* indicar este facto.
*/
export default class ListComponent extends React.Component{

	render(){
		if (this.props.list && this.props.list.length > 0){
			return (<div className="list-group">
		        	{ this.props.list.map(this.props.mapItems) }
		    </div>);
		}else{
			return (<div>
						<p><b>We could not find any resources to display here..</b></p>
					</div>);
		}
	}
}

ListComponent.propTypes = {
	list: React.PropTypes.array.isRequired,
	mapItems: React.PropTypes.func.isRequired
}
