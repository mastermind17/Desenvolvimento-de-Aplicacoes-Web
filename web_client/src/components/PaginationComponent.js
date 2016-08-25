import React from 'react';

/**
* Este componente representa um conjunto de links que permitem
* que o utilizador possa progredir ou retroceder na lista de recursos
* que suportam paginação. Na API, os recursos que suportam esta funcionalidade
* são os projetos e os issues.
*/
export default class PaginationComponent extends React.Component{

	_mapNodesIntoBadges(){
		return this.props.links.map((obj,idx) => {
			if (obj.rel === 'next'){
				return (
					<li key={idx} className="pager-next">
						<a onClick={(evt) => {
						 		evt.preventDefault();
						   		this.props.onPageChange(evt.target.href);
						   	}}
						   	href={obj.href}>
						   	{obj.prompt}
					   	</a>
					</li>);
			}else if (obj.rel === 'previous'){
				return (
					<li key={idx} className="pager-prev">
						<a onClick={(evt) => {
						 		evt.preventDefault();
						   		this.props.onPageChange(evt.target.href);
						   	}}
						   	href={obj.href}>
						   		{obj.prompt}
					   	</a>
					</li>);
			}
		});
	}

	render(){
		if(!this.props.links){
			return null;
		}

		let badges = this._mapNodesIntoBadges();

		return (<nav>
				  <ul className="pager">
				  	{badges}
				  </ul>
				</nav>);
	}
}
